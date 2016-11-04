
class stepperMotor_RI
{
private:
	//Conexao do motor
	int orange, yellow, pink, blue;
	//Caracteristicas do motor
	const int MAX_CNT_STEPS = 2038; //Nmr de passos para 1 volta em full-step
	const double STEP_ANGLE_DEGREE = 0.17664376840039253; //em microradianos
	const double DEGREE2RAD = 0.00308301536171737;
	//const double pi = 3.14159265359;
	//const double angle_step_degree = 0.17665361523586398;
	//Giro do Motor
	bool _half_step; //TRUE = motor girando em half step
	int _sps; //Steps per Second
	bool _spin_dir; //TRUE = ANTI_HORARIO
	unsigned long _delay_ms; //delay em cada passo
	//Posicao do Motor
	unsigned long _step_cnt; //Passos em uma volta
	unsigned long _pos_angle; //Posicao em graus
	short _step_cycle_cnt; //Posicao no ciclo de passos

	void half_step()
	{
		switch(_step_cycle_cnt)
		{
		case 0:
			digitalWrite(orange,1);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		case 1:
			digitalWrite(orange,1);
			digitalWrite(yellow,1);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		case 2:
			digitalWrite(orange,0);
			digitalWrite(yellow,1);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		case 3:
			digitalWrite(orange,0);
			digitalWrite(yellow,1);
			digitalWrite(pink  ,1);
			digitalWrite(blue  ,0);
			break;
		case 4:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,1);
			digitalWrite(blue  ,0);
			break;
		case 5:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,1);
			digitalWrite(blue  ,1);
			break;
		case 6:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,1);
			break;
		case 7:
			digitalWrite(orange,1);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,1);
			break;
		default:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		}
		delay(_delay_ms);

		if(_spin_dir)
		{
			_step_cycle_cnt++;
			_step_cycle_cnt = _step_cycle_cnt > 7 ? 0 : _step_cycle_cnt;
		}
		else
		{
			_step_cycle_cnt--;
			_step_cycle_cnt = _step_cycle_cnt < 0 ? 7 : _step_cycle_cnt;
		}
	}

	void full_step()
	{
		switch(_step_cycle_cnt)
		{
		case 0:
			digitalWrite(orange,1);
			digitalWrite(yellow,1);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		case 1:
			digitalWrite(orange,0);
			digitalWrite(yellow,1);
			digitalWrite(pink  ,1);
			digitalWrite(blue  ,0);
			break;
		case 2:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,1);
			digitalWrite(blue  ,1);
			break;
		case 3:
			digitalWrite(orange,1);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,1);
			break;
		default:
			digitalWrite(orange,0);
			digitalWrite(yellow,0);
			digitalWrite(pink  ,0);
			digitalWrite(blue  ,0);
			break;
		}
		delay(_delay_ms);

		if(_spin_dir)
		{
			_step_cycle_cnt++;
			_step_cycle_cnt = _step_cycle_cnt > 3 ? 0 : _step_cycle_cnt;
		}
		else
		{
			_step_cycle_cnt--;
			_step_cycle_cnt = _step_cycle_cnt < 0 ? 3 : _step_cycle_cnt;
		}
	}
public:
	stepperMotor_RI(int pps=250, bool spin_dir=true,bool is_half_step=false,int pin1=4, int pin2=5, int pin3=6, int pin4=7)
	{
		//Seta a conexao dos pinos
		setPins(pin4, pin3, pin2, pin1);
		//Seta o step mode
		stepMode(is_half_step);
		//Seta a velocidade
		set_sps(pps);
		//Seta a direcao de giro
		setDirection(spin_dir);
		//Seta a posicao atual
		setCurrentPos(0);

	}

	void setDirection(bool dir)
	{
		_spin_dir = dir;
		_step_cnt = 0;
	}

	void set_sps(int new_sps)
	{
		_sps = new_sps;
		_delay_ms = 1000/_sps;
		_delay_ms = _half_step ? _delay_ms/2 : _delay_ms;
	}

	void stepMode(int is_half_step)
	{
		_half_step = is_half_step;
		_step_cnt = 0;
		_step_cycle_cnt = 0;
	}

	void setCurrentPos(double angle, int unit=1)
	{
		//1 - angle em degrees
		//2 - angle em radians
		if(unit==1)
		{
			_pos_angle = angle;
		}
		else
		{
			_pos_angle = angle/DEGREE2RAD;
		}
	}

	void setPins(int ora, int yel, int pin, int blu)
	{
		orange = ora;
		yellow = yel;
		pink   = pin;
		blue   = blu;

		pinMode(orange, OUTPUT);
		pinMode(yellow, OUTPUT);
		pinMode(pink, OUTPUT);
		pinMode(blue, OUTPUT);
	}

	int countSteps()
	{
		return _step_cnt;
	}

	int countStepsCycle()
	{
		return _step_cycle_cnt;
	}

	double getCurrentPos(int unit=1)
	{
		//1 - degrees
		//2 - radians
		if(unit==1)
		{
			return double(_pos_angle);
		}
		else
		{
			return double(_pos_angle*DEGREE2RAD);
		}
	}


	void stopMotor()
	{
		digitalWrite(orange,0);
		digitalWrite(yellow,0);
		digitalWrite(pink  ,0);
		digitalWrite(blue  ,0);
	}



	void stepRun()
	{
		if(_half_step)
			half_step();
		else
			full_step();
		stopMotor();

		_pos_angle = _pos_angle + STEP_ANGLE_DEGREE*(_spin_dir ? 1 : -1);
		_step_cnt++;
		_step_cnt = _half_step ? (_step_cnt >= 2*MAX_CNT_STEPS ? 0 : _step_cnt) : (_step_cnt >= MAX_CNT_STEPS ? 0 : _step_cnt);

                
	}


	void run(double delta_degree)
	{
		setDirection(_spin_dir);
		if(delta_degree > 360)
			delta_degree = delta_degree - int(delta_degree / 360)*360;
		int steps = int(delta_degree/STEP_ANGLE_DEGREE);
		if(_half_step)
		{
			for(int steps_count = 0; steps_count<=2*steps;steps_count++)
				stepRun();
		}
		else
		{
			for(int steps_count = 0; steps_count<=steps;steps_count++)
				stepRun();
		}
                
		stopMotor();
	}

};

bool dir = true;
stepperMotor_RI motor(250, dir, false, 4, 5, 6, 7);

void setup()
{
  Serial.begin(9600);
}

void loop()
{
    motor.run(90);   
    
}
