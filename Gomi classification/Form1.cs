using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using WMPLib;
using System.Threading;

namespace Gomi_classification
{
    
    public partial class Form1 : Form

    {
        private static List<string> trainImages; //keep path to folder

        private static MemoryStream testImage; //keep only one pic

        String[] nametag = new String[] {"","","","",""};
        double[] probability = new double[] {0,0,0,0,0};

        int index = 0;

        VideoCapture capture;
        WindowsMediaPlayer sound;

        // Add your training key from the settings page of the portal
        string trainingKey = "fce9ca3436c74eadbc29d151c40c01ad";
        // Add your prediction key from the settings page of the portal
        // The prediction key is used in place of the training key when making predictions
        string predictionKey = "f21b81b4077e4d81b24172ee461f41e6";

        Guid[] newTagGuid = new Guid[] { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
        private object anotherObject;

        public Form1()
        {
            InitializeComponent();
            Run();

        }

        public void UploadTraing()
        {
            // Create the Api, passing in the training key //conect with could
            TrainingApi trainingApi = new TrainingApi() { ApiKey = trainingKey };

            // Create a prediction endpoint, passing in obtained prediction key
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = predictionKey };
            // connect to project
            var projects = trainingApi.GetProjects();
            var project = projects.FirstOrDefault(p => p.Name == "gomi");

            // looking for tag in project
            int count = 0;
            var allTags = trainingApi.GetTags(project.Id);
            foreach (var tagname in allTags.Tags)
            {
                newTagGuid[count] = tagname.Id;
                Console.WriteLine(newTagGuid[count]);
                count++;
            }

            //Console.WriteLine("Uploading image");
            //label2.Text = "Status: Uploading image";

            trainImages = Directory.GetFiles("C:/Users/Plengjula/source/repos/Gomi classification/Gomi classification/bin/Debug","*.jpg").ToList();

            foreach (var image in trainImages)
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(image)))
                {
                    trainingApi.CreateImagesFromData(project.Id, stream, new List<string>() { newTagGuid[index].ToString() }); //[index]=tag
                }
            }
            Console.WriteLine("Uploading Done");


            /************************ Trainning *****************************/
            ////Now there are images with tags start training the project
            Console.WriteLine("\tTraining");
            var iteration = trainingApi.TrainProject(project.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(1000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(project.Id, iteration.Id);
            }

            // The iteration is now trained. Make it the default project endpoint
            iteration.IsDefault = true;
            trainingApi.UpdateIteration(project.Id, iteration.Id, iteration);
            Console.WriteLine("Done!\n");
            /****************************************************************/



        }

        private void Run()
        {
            try
            {
                capture = new VideoCapture();

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                return;

            }
            Application.Idle += ProcessFrame;
        }
        private void ProcessFrame(object sender, EventArgs e) //ถ่ายรูปซ้ำๆจนเป็นวิดิโอ
        {
            imageBox1.Image = capture.QuerySmallFrame(); 
        }

       
        private static void LoadImagesFromDisk()
        {
            // this loads the images to be uploaded from disk into memory
            trainImages = Directory.GetFiles(@"..\..\..\Images\bottle").ToList(); //choose path
            testImage = new MemoryStream(File.ReadAllBytes(@"..\..\..\Images\Test\test_image.jpg"));//specific on name of pic
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //button1.Enabled = true;
            //button2.Enabled = false;
            //BottleForm bt = new BottleForm(); //คือไรนะ
            //bt.Show();
            bottle form2 = new bottle();
            form2.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //button1.Enabled = true;
            //button3.Enabled = false;
            Can form2 = new Can();
            form2.Show();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //button1.Enabled = true;
            //button4.Enabled = false;
            Box form2 = new Box();
            form2.Show();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            //button1.Enabled = true;
            //button5.Enabled = false;
            burnable form2 = new burnable();
            form2.Show();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            //button1.Enabled = true;
            //button6.Enabled = false;
            Glass form2 = new Glass();
            form2.Show();

        }


        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            int width = Convert.ToInt32(imageBox1.Width);
            int height = Convert.ToInt32(imageBox1.Height);
            Bitmap bmp = new Bitmap(width, height);
            imageBox1.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
            bmp.Save("image.jpg", ImageFormat.Jpeg);

            // Create the Api, passing in the training key //conect with could
            TrainingApi trainingApi = new TrainingApi() { ApiKey = trainingKey };

            // Create a prediction endpoint, passing in obtained prediction key
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = predictionKey };

            /*************** Get Project ***********************/
            var projects = trainingApi.GetProjects();
            var project = projects.FirstOrDefault(p => p.Name == "gomi");

            testImage = new MemoryStream(File.ReadAllBytes("image.jpg"));//specific on name of pic


            //Console.WriteLine("Making a prediction:");
            //label2.Text = ("Status: Making a prediction");
            var result = endpoint.PredictImage(project.Id, testImage);

            // Loop over each prediction and write out the results

            int x = 0;
            foreach (var c in result.Predictions)
            {
                nametag[x] = c.Tag;
                probability[x] = c.Probability;
                Console.WriteLine($"\t{c.Tag}: {c.Probability:P1}");
                x = x + 1;
            }

            if (nametag[0] == "Bottle")
            {
                index = 0; //order change when we create new project 
                button2.Enabled = true;
                //button2.BackColor = Color.Red;
                //var p = new Point(500, 36);
                //this.panel1.Location = p;
                pictureBox1.Enabled = false;
                sound = new WindowsMediaPlayer();
                sound.URL = Application.StartupPath + @"\Mp3\bottle.mp3";
                sound.controls.play();

            }
            else if (nametag[0] == "Can")
            {
                index = 2;
                button3.Enabled = true;
                pictureBox1.Enabled = false;
                // var p = new Point(362, 158);
                //this.panel1.Location = p;       //change panel to this botton
                sound = new WindowsMediaPlayer();
                sound.URL = Application.StartupPath + @"\Mp3\can.mp3";
                sound.controls.play();

            }
            else if (nametag[0] == "Box")
            {
                index = 3;
                button4.Enabled = true;
                pictureBox1.Enabled = false;
                sound = new WindowsMediaPlayer();
                sound.URL = Application.StartupPath + @"\Mp3\box.mp3";
                sound.controls.play();

            }
            else if (nametag[0] == "Burn")
            {
                index = 4;
                button5.Enabled = true;
                pictureBox1.Enabled = false;
                sound = new WindowsMediaPlayer();
                sound.URL = Application.StartupPath + @"\Mp3\burnable.mp3";
                sound.controls.play();

            }

            else if (nametag[0] == "GlassBottle")
            {
                index = 1;
                button6.Enabled = true;
                pictureBox1.Enabled = false;
                sound = new WindowsMediaPlayer();
                sound.URL = Application.StartupPath + @"\Mp3\glass.mp3";
                sound.controls.play();

            }
            Console.WriteLine(nametag[0]);
            pictureBox2.Enabled = true;

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to trend this image to the system?", "System Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                pictureBox1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                pictureBox2.Enabled = false;
                UploadTraing();

            }
            else if (result == DialogResult.No)
            {
                pictureBox1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                pictureBox2.Enabled = false;
            }

            

        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to trend this image to the system?", "System Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                pictureBox1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                pictureBox2.Enabled = false;
                UploadTraing();

            }
            else if (result == DialogResult.No)
            {
                pictureBox1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                pictureBox2.Enabled = false;
            }


        }
    }
}
