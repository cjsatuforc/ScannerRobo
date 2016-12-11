using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using's para Camera
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;

//using's para Comunicacao Serial
using System.IO.Ports;
using System.IO;

namespace ControleHardware01
{
    public partial class Controlador : Form
    {

        int posY = 0;
        int posFinalY = 0;
        int posFinalX = 0;
      

        public Controlador()
        {
            InitializeComponent();
            posFinalY = imgCamera.Height;
            posFinalX = imgCamera.Width;
            btnVarredura.Text = "Varredura";

        }

        Camera camera = new Camera();
        Thread comunicacaoSerial;
        Thread cameraView;
        Thread varredura;
        bool flagPararVarredura = false;

        bool chave1 = false;


    
        #region funções para comunicacao serial

        //metodo para atualizar comboBox de COM
        private void atualizaListaComPorts()
            {
                int i=0;
                bool quantidadeDiferente=false; //flag para sinalizar que a quantidade de portas mudou

                //se a quantidade de portas mudou
                if (comboBox1.Items.Count == SerialPort.GetPortNames().Length)
                {
                    foreach (string s in SerialPort.GetPortNames())
                    {
                        if (comboBox1.Items[i++].Equals(s) == false)
                            quantidadeDiferente = true;
                    }
                }
                else
                    quantidadeDiferente = true;

                if (quantidadeDiferente == false)
                    return;

                //limpa comboBox1
                comboBox1.Items.Clear();

                foreach(string s in SerialPort.GetPortNames())
                {
                    comboBox1.Items.Add(s); //adiciona COM a lista da comboBox1
                }

                //seleciona a primeira da lista
                comboBox1.SelectedIndex = 0;
            }

            //metodo para validar comunicação serial
            private void enviaMicro_Validacao()
            {
                serialPort1.Write("finatec");
            }
            
            //metodo para ligar laser movel
            private void enviarLigarLaserMovel()
            {
                serialPort1.Write("ligar laser movel");
            }

            //metodo para desligar laser movel
            private void enviarDesligarLaserMovel()
            {
               serialPort1.Write("desligar laser movel");
            }

            //metodo para ligar laser fixo
            private void enviarLigarLaserFixo()
            {
                this.serialPort1.Write("ligar laser fixo");
            }

            //metodo para desligar laser fixo
            private void enviarDesligarLaserFixo()
            {
                serialPort1.Write("desligar laser fixo");
            }

            //metodo para girar motor sentido horario
            private void enviarMotorHorario()
            {
                serialPort1.Write("ligar motor horario");
            }

            //metodo para girar motor sentido horario rapidamente
            private void enviarMotorHorarioRapido()
            {
                serialPort1.Write("ligar motor horario rapido");
            }

            //metodo para girar motor sentido anti-horario
            private void enviarMotorAntiHorario()
            {
                serialPort1.Write("ligar motor antihorario");
            }

            //metodo para girar motor sentido anti-horario rapidamente
            private void enviarMotorAntiHorarioRapido()
            {
                serialPort1.Write("ligar motor antihorario rapido");
            }

            //metodo para parar motor
            private void enviarMotorParar()
            {
                serialPort1.Write("parar motor");
            }

        #endregion

        public void initCamera()
            {
                if (camera.dispositivoExistente) //verifica a flag da camera 
                {
                    camera.deviceCamera = new VideoCaptureDevice(camera.dispositivoVideo[cbCameras.SelectedIndex].MonikerString); //seleciona fonte de video da comboBox
                    //chama metodo q atualiza ImgView
                    cameraView = new Thread(new ThreadStart(this.Video));
                    cameraView.IsBackground = true;
                    cameraView.Start();
                    
                    toolStripStatusLabel1.Text = "Camera inicializada";
                    btnCamera.Text = "Parar Camera"; //atualiza estado do btn
                }
            }

        public void Video()
        {
            camera.deviceCamera.NewFrame += new NewFrameEventHandler(video_NewFrame);
            camera.deviceCamera.Start(); //abre a camera
        }

        //função para listar cameras existentes
        public void listarCamera()
            {



                cbCameras.Items.Clear();
                try
                {
                    camera.dispositivoVideo = new FilterInfoCollection(FilterCategory.VideoInputDevice); //aloca um dispositivo de video

                    if (camera.dispositivoVideo.Count == 0) throw new ApplicationException(); //encerra programa se nao alocar o vetor de videos

                    camera.dispositivoExistente = true; //flag para ver se existe algum dispositivo de video
                    foreach (FilterInfo dispositivo in camera.dispositivoVideo) // loop para varrer "vetor" e adicionar na comboBox
                    {

                        cbCameras.Items.Add(dispositivo.Name); //adiona na comboBox
                    }
                    cbCameras.SelectedIndex = 0; //tornar padrao
                }
                catch (ApplicationException) // tratamento de erro
                {
                    camera.dispositivoExistente = false;
                    cbCameras.Items.Add("Nenhum dispositivo de camera encontrado");
                }
            }

        //metodo q atualiza a PictureBox
        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {       
                Bitmap img = (Bitmap)eventArgs.Frame.Clone();
                imgCamera.Image = img;
                Thread.Sleep(10);
                camera.imageCamera = img;
                Thread.Sleep(10);
        }

        //fechar camera com segunrança
        public void PararVideo()
            {
                if (!(camera == null)) // verifica se a camera esta aberta
                    if (camera.deviceCamera.IsRunning) //verifica se esta rodando
                    {
                        camera.deviceCamera.SignalToStop(); // para video
                        camera = null; //limpa camera
                        imgCamera.Image = null; //limpa pictureBox
                    }
            }


        void ImagemParalela_04(Bitmap bmpMovel, string Referencia, int numeroimagem)
        {
            try
            {

                Bitmap bmp01 = new Bitmap(Referencia);

                Bitmap bmp = (Bitmap)bmpMovel.Clone();

                // Lock the bitmap's bits.  
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);


                Rectangle rect01 = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData01 =
                    bmp01.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp01.PixelFormat);



                IntPtr ptr = bmpData.Scan0;
                IntPtr ptr01 = bmpData01.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
                byte[] rgbValues = new byte[bytes];

                int bytes01 = bytes;
                byte[] rgbValues01 = new byte[bytes];




                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                System.Runtime.InteropServices.Marshal.Copy(ptr01, rgbValues01, 0, bytes01);

                // Set every third value to 255. A 24bpp bitmap will look red.  

                int filtro = 0;

                for (int counter = 2; counter < rgbValues.Length; counter += 3)
                {

                    filtro= (byte)Math.Abs((rgbValues[counter] - rgbValues01[counter]));

                    if (filtro <50) filtro = 0;
                    else filtro = 255;

                    rgbValues[counter - 2] = (byte)filtro; 
                    rgbValues[counter-1] = (byte)filtro;
                    rgbValues[counter] = (byte)filtro;


                    /*
                                        rgbValues[counter - 2] = (byte)Math.Abs((rgbValues[counter - 2] - rgbValues01[counter - 2]));//R
                                        rgbValues[counter - 1] = (byte)Math.Abs((rgbValues[counter - 1] - rgbValues01[counter - 1])); //G
                                        rgbValues[counter] = (byte)Math.Abs((rgbValues[counter] - rgbValues01[counter])); //B

                        */


                }



                // Copy the RGB values back to the bitmap
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                //  bmp.Save(v_NomeSerieImagens_base + numeroimagem + "movel_04.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                string nomearquivosaida = "";
                if (numeroimagem == -1)
                {
                    //SALVAR IMAGEM LASER FIXO
                    nomearquivosaida = "C:\\temp\\ImagensOriginais\\_IMG_BUFF_F.bmp";
                }

                else {

                    if (numeroimagem == -2)
                    {
                        //SALVAR IMAGEM LASEr movel
                        nomearquivosaida = "C:\\temp\\ImagensOriginais\\_IMG_BUFF_.bmp";

                    }

                    else
                    {
                        nomearquivosaida = "C:\\temp\\ImagensOriginais\\_IMG_BUFF_" + numeroimagem + ".bmp";

                    }
                }


               

                bmp.Save(nomearquivosaida); //salva fotos com laser movel


                //    ContadorUniversal++;


            }

            catch (Exception ex)
            {
                var erro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Falha no tratamento da imagem. Erro: " + erro);

            }



        }



        //metodo para salvar foto
        public bool salvarImagem(Bitmap img, string end, int sel, int cont)
        {
            try 
            {
                if (sel == 0)
                {
                    Thread.Sleep(100);

                    ImagemParalela_04(img, @"" + end + "_IMG_BUFF_BASE.bmp", cont);

                }
                else if (sel == 1)
                {

                    ////o valor de -1 é para salva foto com laser fixo
                    ////img.Save(@"" + end + "_IMG_BUFF_F.bmp"); // salva foto com laser fixo


                    ImagemParalela_04(img, @"" + end + "_IMG_BUFF_BASE.bmp", -1);
                                   }
                else
                {
                    if (sel == 2)
                    {

                        ////o valor de -2 é para salva foto com laser movel
                        ////     img.Save(@"" + end + "_IMG_BUFF_.bmp"); // salva foto com laser Movel

                        ImagemParalela_04(img, @"" + end + "_IMG_BUFF_BASE.bmp", -2);

                   

                    }
                    else
                    {
                        img.Save(@"" + end + "_IMG_BUFF_BASE.bmp");
                    }
                }
                

                return true;
            }
            catch (Exception ex)
            {
                var erro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //limpa comboBox para listar cameras
            cbCameras.Items.Clear();
            timerCOM.Enabled = true;
            listarCamera(); //chamada de função da classe Camera para adcionar cameras existentes
        }

        private void btnCamera_Click(object sender, EventArgs e)
        {
            if (btnCamera.Text == "Iniciar Camera") //verifica estado do btn
            {
                initCamera();
            }
            else
            {
                PararVideo(); //stop video
                toolStripStatusLabel1.Text = "Camera interrompida";
                btnCamera.Text = "Iniciar Camera"; //atualiza estado do btn
            }
        }


        private void salvarNome() {
            
           

           Directory.CreateDirectory(@"C:\temp\ImagensOriginais");



            // folderBrowserDialog1.ShowDialog(); //abre menu para seleiconar diretorio
            // string end = Convert.ToString(folderBrowserDialog1.SelectedPath); //guarda endereco do diretorio selecionado
            String end = "C:\\temp\\ImagensOriginais";
            camera.enderecoImagem = end + "\\";
            toolStripStatusLabel1.Text = "Diretorio selecionado com sucesso";
        
        }
        

        private void salvarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
                    
                   

        
  
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serialPort1.Close();

            PararVideo();//para video
            Application.Exit(); //fecha applicacao
        }

        private void timerCOM_Tick(object sender, EventArgs e)
        {
            atualizaListaComPorts();


        }

        private void btnComunicaoSerial_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == false)
            {
                try
                {
                    serialPort1.PortName = comboBox1.Items[comboBox1.SelectedIndex].ToString(); //pega nome da COM
                    serialPort1.BaudRate = 9600; //define velocidade de transmissao
                    serialPort1.Open(); //abre comunicação serial

                    //cria uma thread para comunicacao serial
                    this.comunicacaoSerial = new Thread(new ThreadStart(this.enviaMicro_Validacao));
                    this.comunicacaoSerial.IsBackground = true;
                    this.comunicacaoSerial.Start(); //inicia thread
                }
                catch
                {
                    return;
                }
                if (serialPort1.IsOpen)
                {
                    btnComunicaoSerial.Text = "Desconectar";
                    comboBox1.Enabled = false;
                }
            }
            else
            {
                try
                {
                    serialPort1.Close();
                    comboBox1.Enabled = true;
                    btnComunicaoSerial.Text = "Iniciar conexão";
                }
                catch
                {
                    return;
                }
            }
        }

        private void btnAntiHorario_Click(object sender, EventArgs e)
        {
            this.comunicacaoSerial = new Thread(new ThreadStart(this->enviarMotorAntiHorarioRapido));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
        }

        private void btnParar_Click(object sender, EventArgs e)
        {
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarMotorParar));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
        }

        private void btnHorario_Click(object sender, EventArgs e)
        {
            this.comunicacaoSerial = new Thread(new ThreadStart(this->enviarMotorHorarioRapido));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread 
        }

        private void btnLaserFixo_Click(object sender, EventArgs e)
        {
             //verifica se a porta está aberta se não imprimi em uma caica de dialogo
            if (serialPort1.IsOpen)
            {
                if(btnLaserFixo.Text == "Ligar")
                {
                    //cria thread para comunicao serial
                    this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarLigarLaserFixo));
                    this.comunicacaoSerial.Start(); //inicia thread

                    btnLaserFixo.Text = "Desligar";
                    toolStripStatusLabel1.Text = "Laser Fixo ligado.";
                }
                else
                {
                    //cria thread para comunicao serial
                    this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserFixo));
                    this.comunicacaoSerial.Start(); //inicia thread
                    
                    btnLaserFixo.Text = "Ligar";
                    toolStripStatusLabel1.Text = "Laser Fixo desligado.";
                }
            }

        }

        private void btnLaserMovel_Click(object sender, EventArgs e)
        {
            //verifica se a porta está aberta se não imprimi em uma caica de dialogo
            if (serialPort1.IsOpen)
            {
                if (btnLaserMovel.Text == "Ligar")
                {
                    //cria thread para comunicao serial
                    this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarLigarLaserMovel));
                    this.comunicacaoSerial.Start(); //inicia thread

                    btnLaserMovel.Text = "Desligar";
                    toolStripStatusLabel1.Text = "Laser Movel ligado.";
                }
                else
                {
                    //cria thread para comunicao serial
                    this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserMovel));
                    this.comunicacaoSerial.Start(); //inicia thread

                    btnLaserMovel.Text = "Ligar";
                    toolStripStatusLabel1.Text = "Laser Movel desligado.";
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            serialPort1.Close();
       
            PararVideo();//para video
            Application.Exit(); //fecha applicacao
        }

        private void btnVarredura_Click(object sender, EventArgs e)
        {
            salvarNome();

          

            if (btnVarredura.Text == "Varredura")
            {
                varredura = new Thread(new ThreadStart(this.varreduraThread));
                varredura.IsBackground = true;
                varredura.Start();
                btnVarredura.Text = "Parar Varredura";
                btnVarredura.BackColor = Color.Red;

          
            }
            else
            {
                btnVarredura.Text = "Varredura";
                flagPararVarredura = true;
                btnVarredura.BackColor = Color.Blue;


            }
        }

        public void varreduraThread()
        {
            //**********************************************************
            //desliga laser movel e desliga laser fixo
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserMovel));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserFixo));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            bool sucesso0;

            do
            {
                sucesso0 = salvarImagem((Bitmap)camera.imageCamera, camera.enderecoImagem, 3, 0);
            } while (!sucesso0);

            



            //**********************************************************
            //desliga laser movel e tira foto
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserMovel));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarLigarLaserFixo));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            bool sucesso1;

            do
            {
                sucesso1 = salvarImagem((Bitmap)camera.imageCamera, camera.enderecoImagem, 1, 0);
            } while (!sucesso1);

            //**********************************************************
            //desligar laser fixo, liga laser movel e tira foto
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarDesligarLaserFixo));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarLigarLaserMovel));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread
            Thread.Sleep(1000);

            bool sucesso2;

            do
            {
                sucesso2 = salvarImagem((Bitmap)camera.imageCamera, camera.enderecoImagem, 2, 0);
            } while (!sucesso2);

            //**********************************************************
            //logica de varredura
            //aciona motor no sentido desejado e tirar fotos
            flagPararVarredura = false;
            //liga motor
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarMotorHorario));  ///direçao correta
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread

            for (int i = 0; !flagPararVarredura; i++ )
            {
                var sucesso = salvarImagem((Bitmap)camera.imageCamera, camera.enderecoImagem, 0, i);
                if (!sucesso) i -= 1;
            }

            //para motor
            this.comunicacaoSerial = new Thread(new ThreadStart(this.enviarMotorParar));
            this.comunicacaoSerial.IsBackground = true;
            this.comunicacaoSerial.Start(); //inicia thread

            //**********************************************************



            camera.flag = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (chave1) {

               

           


            }
        }

        private void Calibrar_Click(object sender, EventArgs e)
        {

            chave1 = true;

        
            imgCamera.Invalidate();


        }

        private void imgCamera_Paint(object sender, PaintEventArgs e)
        {

            if (chave1)
            {
                e.Graphics.DrawLine(
                new Pen(Color.Red, 2f),
                new Point(0, posY),
                new Point(posFinalX, posY));
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            posY-=2;

            if (posY < 0) posY = 0;
            if (posY > posFinalY) posY = posFinalY;
            imgCamera.Invalidate();



        }

        private void button2_Click(object sender, EventArgs e)
        {
            posY+=2;

            if (posY < 0) posY = 0;
            if (posY > posFinalY) posY = posFinalY;
            imgCamera.Invalidate();


        }

        private void ApagarMemoria_Click(object sender, EventArgs e)
        {
            
            try
            {
                if (Directory.Exists(@"C:\temp\ImagensOriginais"))
                {

                    Directory.Delete(@"C:\temp\ImagensOriginais", true);



                }



                if (Directory.Exists(@"C:\temp\redimensaoimagem"))
                {


                    Directory.Delete(@"C:\temp\redimensaoimagem", true);


                }


                if (Directory.Exists(@"C:\temp\pastatemporal"))
                {


                    Directory.Delete(@"C:\temp\pastatemporal", true);


                }
            }



            catch (ApplicationException) // tratamento de erro
            {

                MessageBox.Show("Falha");
            }


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chave1 = chave1 == false ? true : false;
            imgCamera.Invalidate();



        }
    }

    public class Camera 
    {
        public bool dispositivoExistente = false; // verrifica se tem camera
        public VideoCaptureDevice deviceCamera; //fonte de video
        public FilterInfoCollection dispositivoVideo; //dispositivo
        public string enderecoImagem; //string para armazenar endereco para salvar fotos
        public Image imageCamera;
        public bool flag = false;
    }

    
}
