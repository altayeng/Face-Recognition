using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BitirmeProject
{
    class BusinessRecognition
    {
        string Dizin;
        string KlasorAdi;
        string XmlVeriDosyasi;

        public BusinessRecognition(string Dizin, string KlasorAdi, string XmlVeriDosyasi)
        {
            this.Dizin = Dizin + "/" + KlasorAdi + "/";
            this.XmlVeriDosyasi = XmlVeriDosyasi;
            Eigen_Recog = new Classifier_Train(Dizin, KlasorAdi, XmlVeriDosyasi);
        }

        //Arka Arkaya 10 Görüntü Yakalamak İçin
        List<Image<Gray, byte>> resultImages = new List<Image<Gray, byte>>();

        //Eğitim Sınıflandırıcısı
        Classifier_Train Eigen_Recog;
 
        // XML Veri Dosyaları
        XmlDocument docu = new XmlDocument();
 
        // Veri Kaydet
        public bool SaveTrainingData(Image face_data, string FaceName)
        {
            try
            {
                string NAME_PERSON = FaceName;
                Random rand = new Random();
                bool file_create = true;
                string facename = "face_" + NAME_PERSON + "_" + rand.Next().ToString() + ".jpg";
                while (file_create)
                {
                    //dosya içinde Dizin diye bir şey var mı kontrol ediyoruz. Varsa yazılan isimle kayıt ediyoruz
                    if (!File.Exists(Dizin + facename))
                    {
                        file_create = false;
                    }
                    else
                    {
                        facename = "face_" + NAME_PERSON + "_" + rand.Next().ToString() + ".jpg";
                    }
                }

                //Directory kısmında Dizin adında bir şey varsa fotoğrafı kaydettiriyoruz
                if (Directory.Exists(Dizin))
                {
                    face_data.Save(Dizin + facename, ImageFormat.Jpeg);
                }
                else
                {
                    Directory.CreateDirectory(Dizin);
                    face_data.Save(Dizin + facename, ImageFormat.Jpeg);
                }
                if (File.Exists(Dizin + XmlVeriDosyasi))
                {
                    //dizin klasöründe Xmlveridosyası adında bir şey var mı onu kontrol ediyoruz. Eğer yoksa oluşturuyoruz
                    bool loading = true;
                    while (loading)
                    {
                        try
                        {
                            docu.Load(Dizin + XmlVeriDosyasi);
                            loading = false;
                        }
                        catch
                        {
                            docu = null;
                            docu = new XmlDocument();
                            Thread.Sleep(10);
                        }
                    }

                    //Get the root element
                    XmlElement root = docu.DocumentElement;

                    XmlElement face_D = docu.CreateElement("FACE");
                    XmlElement name_D = docu.CreateElement("NAME");
                    XmlElement file_D = docu.CreateElement("FILE");

                    //her bir kayıta (veya düğüme) atama yapıyoruz
                    name_D.InnerText = NAME_PERSON;
                    file_D.InnerText = facename;

                   
                    face_D.AppendChild(name_D);
                    face_D.AppendChild(file_D);
 
                    root.AppendChild(face_D);

                    //XML dosyasını kayıt
                    docu.Save(Dizin + XmlVeriDosyasi);
             
                }
                else
                {
                    //XML dosyasına isimleri atıyoruz
                    FileStream FS_Face = File.OpenWrite(Dizin + XmlVeriDosyasi);
                    using (XmlWriter writer = XmlWriter.Create(FS_Face))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Faces_For_Training");

                        writer.WriteStartElement("FACE");
                        writer.WriteElementString("NAME", NAME_PERSON);
                        writer.WriteElementString("FILE", facename);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    FS_Face.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
 
        // Kayıtları Verileri Sil
        public void DeleteTrains()
        {
            if (Directory.Exists(Dizin))
            {
                Directory.Delete(Dizin, true);
            }
            Directory.CreateDirectory(Dizin);
        }
     }
}
