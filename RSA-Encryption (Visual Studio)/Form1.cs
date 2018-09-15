using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography; // in order to be able to use the built in libraries
using System.IO;

namespace EECE455_Project_2._0
{   
    public partial class Form1 : Form
    {
        long lengthoffile = 0;
        bool decryptedpressed = false;
        bool encryptpressed = false;
        const int length = 117; // for 1024 bits
        bool imported = false;
        int keysize = 0;
        string Filenamegen = null;
        string batata = null;
        string batata1 = null;
        string arnabit = null;

        byte[] IV0 = new byte[length];

        UnicodeEncoding ByteConverter = new UnicodeEncoding();
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        static public byte[] Encryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding) // RSA Encryption algorithm
        {   
            

            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }
        //a method to generate a random intial vector with a specified length for cbcEncryption 
        public static byte[] iV(int length) // Random IV Generator
        {
            Random rand = new Random();
            byte[] buffer = new byte[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = (byte)rand.Next(256);
            }

            return buffer;
        }
        public void cbcEncryption(String message) // CBC Encryption
        {

            byte[] IV = iV(length);
            IV0 = IV; // Saving the random generated IV

            char ch;
            int counter = 0;
            int stringCounter = 0;// used to check if the string ended or not


            byte[] xor = new byte[length];
            byte[] encryptedText = new byte[length];
            byte[] encryptedBytesOutput = new byte[1+(stringCounter/117)];

 
            
            for (int i = 0; i < length; i++) // just initializing xor[]
                xor[i] = 0; // Padding with zeros/null characters




            for (; stringCounter < message.Length; stringCounter++,counter++)
            {
               
                ch = message[stringCounter];//read from the file character by character until we reach the end

                if (counter >= length)
                {
                    counter = stringCounter % length;

                    encryptedtext = Encryption(xor, RSA.ExportParameters(false), false);
                    setArray(IV, encryptedtext);//setting IV equal to the encrypted text for the next block
                 txtencrypt.Text = txtencrypt.Text + ByteConverter.GetString(encryptedtext);//display original

                    byte[] decryptedData = Decryption(encryptedtext, RSA.ExportParameters(true), false);


                    for (int i = 0; i < length; i++)
                    {
                        decryptedData[i] = Convert.ToByte((Convert.ToInt32(decryptedData[i])) ^ (Convert.ToInt32(IV[i])));
                        batata = Encoding.ASCII.GetString(decryptedData);
                    }

                    batata1 = batata1 + batata;

                }

                try
                {
                    xor[counter] = Convert.ToByte((Convert.ToInt32(ch)) ^ (Convert.ToInt32(IV[counter])));
                }
                catch
                { ATTN.Text = "Attention: Non-ASCII type of characters detected. Output may vary!"; }
                
            }
            // we notice that its outside the while loop
            if (counter != length) // this was meant for less than 1 length
            {
                for (int i = counter; i < length; i++)
                    xor[i] = Convert.ToByte(0 ^ (Convert.ToInt32(IV[i])));//xoring the padding (zero's) with the remaining values of the IV


                encryptedtext = Encryption(xor, RSA.ExportParameters(false), false);

                txtencrypt.Text = txtencrypt.Text + ByteConverter.GetString(encryptedtext);//display

                byte[] decryptedData = Decryption(encryptedtext, RSA.ExportParameters(true), false);

                for (int i = 0; i < length; i++)
                {
                    decryptedData[i] = Convert.ToByte((Convert.ToInt32(decryptedData[i])) ^ (Convert.ToInt32(IV[i])));
                    batata = Encoding.ASCII.GetString(decryptedData);
                }

                batata1 = batata1 + batata;

            }
            else if (counter == length)
            {
                counter = stringCounter % length;

                encryptedtext = Encryption(xor, RSA.ExportParameters(false), false);
                setArray(IV, encryptedtext);//setting IV equal to the encrypted text for the next block
                txtencrypt.Text = txtencrypt.Text + ByteConverter.GetString(encryptedtext);//display

                byte[] decryptedData = Decryption(encryptedtext, RSA.ExportParameters(true), false);


                for (int i = 0; i < length; i++)
                {
                    decryptedData[i] = Convert.ToByte((Convert.ToInt32(decryptedData[i])) ^ (Convert.ToInt32(IV[i])));
                    batata = Encoding.ASCII.GetString(decryptedData);
                }

                batata1 = batata1 + batata;

            }








        }






        
        public void cbcDecryption(String ciphertext, byte[] IV) // CBC Decryption
        {


            IV = iV(length);
            byte[] input = new byte[length];
            byte[] xor = new byte[length];
            byte[] decryptedText = new byte[length];

            char ch;
            int counter = 0;
            int stringCounter = 0; // used to check if the string ended or not

            while (stringCounter < ciphertext.Length)
            {
                ch = ciphertext[counter];//read from the file character by character until we reach the end
                if (counter < length)
                    input[counter] = Convert.ToByte(ch); // ASCII!!!
                else
                {
                    decryptedText = Decryption(input, RSA.ExportParameters(false), false);

                    for (int i = 0; i < length; i++)//xoring the output of the RSA decryption
                        xor[i] = Convert.ToByte((Convert.ToInt32(decryptedText[i])) ^ (Convert.ToInt32(IV[i]))); //P1
                    
                    setArray(IV, input);//setting IV equal to the input text for the next block 
                    txtdecrypt.Text = ByteConverter.GetString(xor);//we dsiplay the xored array 
                    counter = 0;
                    input[counter] = Convert.ToByte(ch);
                }
                counter++;
                stringCounter++;
            }

            if (stringCounter < length) // in case in never went into the else section
            {
                for (int i = stringCounter; i < length; i++)
                    input[i] = 0;//xoring the padding (zero's) with the remaining values of the IV


                decryptedText = Decryption(input, RSA.ExportParameters(true), false);

                txtdecrypt.Text = txtdecrypt.Text + ByteConverter.GetString(decryptedText); //display
            }
        }






        //a method to set the elements of array a equal to the elements of array b without pointer issues
        public void setArray(byte[] a, byte[] b)
        {
            int i = 0;
            while (a.Length == b.Length && i < a.Length)
            {
                a[i] = b[i];
                i++;
            }
        }
        

        static public byte[] Decryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding) // RSA Decryption algorithm
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    decryptedData = RSA.Decrypt(Data, DoOAEPPadding);

                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }

        byte[] plaintext;
        byte[] encryptedtext;



        private void button1_Click_1(object sender, EventArgs e) // Encrypt Button
        {
            if(txtplain.Text!="")
            {
                if (encryptpressed == false)
                {
                    encryptpressed = true;
                    txtdecrypt.Clear();
                    txtencrypt.Clear();
                    ATTN.Text = "";

                    cbcEncryption(txtplain.Text);
                }
                else { ATTN.Text = "Attention: Please Reset for a new Encryption"; }
            }
            else {ATTN.Text = "Attention: Please Type/Browse Plain Text for Encryption, then Press Encrypt";}
        }

        private void button2_Click_1(object sender, EventArgs e) // Decrypt Button
        {
            if (txtencrypt.Text != "")
            {
                if (decryptedpressed == false)
                {
                    decryptedpressed = true;
                    txtdecrypt.Clear();
                    ATTN.Text = "";


                    txtdecrypt.Text = batata1;


                }
                else { ATTN.Text = "Attention: Please Reset for a new Encryption"; }
            }
            else if(txtplain.Text =="") { ATTN.Text = "Attention: Please Import/Type text for Encryption, then Press Encrypt"; }
            else if (encryptpressed == false) { ATTN.Text = "Attention: Please Press Encrypt"; }
        }



        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) // about tool, OK
        {
          creatorsToolStripMenuItem.Text = "Created by:\nHussam Abdul Khalek\nBassel Abou Ali Modad\nJad Aboul Hosn\nSaadallah Kassir\n\nEECE455 Project";
        }


        private void resetToolStripMenuItem_Click(object sender, EventArgs e) // reset tool, Needs to be modified!!
        {
            decryptedpressed = false;
            encryptpressed = false;
            txtplain.Clear();
            txtplain.Text = "";
            txtencrypt.Clear();
            txtencrypt.Text = "";
            txtdecrypt.Clear();
            txtdecrypt.Text = "";
            batata1 = "";
            batata = "";
            ATTN.Text = "";
            imported = false;
            

        }

        private void button3_Click(object sender, EventArgs e) // Browse for plaintext to encrypt
        {
            
            if (encryptpressed == false)
            {
                txtplain.Clear();
                ATTN.Text = "";

                imported = true;
                string filename = null;

           

                    OpenFileDialog fdlg = new OpenFileDialog();
                    fdlg.Title = "Browse a TXT file";
                    fdlg.InitialDirectory = @"c:\";
                    fdlg.Filter = "txt files (*.txt)|*.txt";
                    fdlg.FilterIndex = 2;
                    fdlg.RestoreDirectory = true;

                    if (fdlg.ShowDialog() == DialogResult.OK)
                    {


                        if (!string.IsNullOrEmpty(fdlg.FileName))
                        {
                            filename = fdlg.FileName;
                            string text = System.IO.File.ReadAllText(filename);
                            txtplain.Text = text;
                            Filenamegen = filename;
                        }
                       
                    }
                    else { return; }
                    
                    
                   


                    lengthoffile = new System.IO.FileInfo(filename).Length;
                    if (lengthoffile > 1000)
                        ATTN.Text = "Attention: The Size of file chosen is " + lengthoffile.ToString() + " bytes. This may take a few seconds";



            }
            else { ATTN.Text = "Attention: Please Reset for a new Encryption"; }
        }

        private void txtencrypt_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e) // Export Encrypted Button
        {
            if (txtencrypt.Text != "")
            {
                Random rnd = new Random();
                string Filenamegenexport = null;
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (imported == false)
                {
                    Filenamegenexport = System.IO.Path.Combine(desktop, "RSA-Encrypted" + rnd.Next(1, 50) + ".txt");
                    ATTN.Text = "Please check the saved txt file on your desktop";
                }
                else {

                  
                        Filenamegenexport = Filenamegen + "-Encrypted" + rnd.Next(1, 50) + ".txt";
                        ATTN.Text = "Please check the saved txt file, on the same directory of the chosen file";
               

                }
               
                    try
                    {
                        System.IO.File.WriteAllText(Filenamegenexport, txtencrypt.Text);
                    }
                    catch
                    {
                        Filenamegenexport = System.IO.Path.Combine(desktop, "RSA-Encrypted" + rnd.Next(1, 50) + ".txt");
                        System.IO.File.WriteAllText(Filenamegenexport, txtencrypt.Text);
                        ATTN.Text = "Due to user constraints: Please check the saved txt file on your desktop";
                    }
               
            }
            else { ATTN.Text = "Attention: Please Import/Type text,Press Encrypt and then Export"; }
        }

        private void button5_Click(object sender, EventArgs e) // Export Decrypted Button
        {   
            
         if(txtdecrypt.Text !="")
         {
        
            Random rnd = new Random();
            string Filenamegenexport = null;
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (imported == false)
            {
                Filenamegenexport = System.IO.Path.Combine(desktop, "RSA-Decrypted" + rnd.Next(1, 50) + ".txt");
                ATTN.Text = "Please check the saved txt file on your desktop";
            }
            else { Filenamegenexport = Filenamegen + "-Decrypted" + rnd.Next(1, 50) + ".txt"; 
                ATTN.Text = "Please check the saved txt file, on the same directory of the chosen file"; }





            try
            {
                System.IO.File.WriteAllText(Filenamegenexport, txtdecrypt.Text);
            }
            catch
            {
                Filenamegenexport = System.IO.Path.Combine(desktop, "RSA-Decrypted" + rnd.Next(1, 50) + ".txt");
                System.IO.File.WriteAllText(Filenamegenexport, txtdecrypt.Text);
                ATTN.Text = "Due to user constraints: Please check the saved txt file on your desktop";
            }
          

        }
        else if(txtplain.Text=="")
            { ATTN.Text = "Attention: Please Import/Type text for Encryption, then Press Encrypt"; }
         else if (txtplain.Text != "" && txtencrypt.Text == "")
         { ATTN.Text = "Attention: Please Press Encrypt, then Decrypt"; }
         else { ATTN.Text = "Please press decrypt"; }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            decryptedpressed = false;
            encryptpressed = false;
            txtplain.Clear();
            txtplain.Text = "";
            txtencrypt.Clear();
            txtencrypt.Text = "";
            txtdecrypt.Clear();
            txtdecrypt.Text = "";
            batata1 = "";
            batata = "";
            ATTN.Text = "";
            imported = false;
            
        }


      

    
    
    }

    


}
