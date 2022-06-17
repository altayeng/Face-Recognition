using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace BitirmeProject
{
    class Classifier_Train : IDisposable
    {
        
        string Dizin;
        string KlasorAdi;
        string XmlVeriDosyasi;
        public Classifier_Train(string Dizin, string KlasorAdi)
        {
            this.Dizin = Dizin + KlasorAdi; 
            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            _IsTrained = LoadTrainingData(this.Dizin);
        }
        public Classifier_Train(string Dizin, string KlasorAdi, string XmlVeriDosyasi)
        {
            this.Dizin = Dizin + KlasorAdi;
            this.XmlVeriDosyasi = XmlVeriDosyasi;
              

            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            _IsTrained = LoadTrainingData(this.Dizin);
        }

        #region Değişkenler

        //Eigen
        MCvTermCriteria termCrit;
        EigenObjectRecognizer recognizer;

        //Eğitim için değişkenler
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();//görsel
        List<string> Names_List = new List<string>(); //isim
        int ContTrain, NumLabels;
        float Eigen_Distance = 0;
        string Eigen_label;
        int Eigen_threshold = 0;

       
        string Error;
        bool _IsTrained = false;

        #endregion

        #region Constructors
       
        /// Eğitim seti için Dizin klasörünü aratıyoruz
        
        public Classifier_Train()
        {
            KlasorAdi = "TrainedFaces";
            Dizin = Application.StartupPath + "\\" + KlasorAdi;
            XmlVeriDosyasi = "TrainedLabels.xml";

            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            _IsTrained = LoadTrainingData(Dizin);
        }

         /// Eğitim seti için girilen değeri, başka bir değişkene atıyor ki train esnasında zorluk olmasın.
          public Classifier_Train(string Training_Folder)
        {
            termCrit = new MCvTermCriteria(ContTrain, 0.001);
            _IsTrained = LoadTrainingData(Training_Folder);
        }
        #endregion

        #region Public
        //Eğitim tamamlanmış mı kontrol ediyor, bool değer döndürür
        public bool IsTrained
        {
            get { return _IsTrained; }
        }

       
        // Train ettiğpimiz Eigen Gri Level'da Tanıma yapar
 
        public string Recognise(Image<Gray, byte> Input_image, int Eigen_Thresh = -1)
        {
            if (_IsTrained)
            {
                EigenObjectRecognizer.RecognitionResult ER = recognizer.Recognize(Input_image);
                //Ekranda 2 kişi var ise eğitilmeyen kişiye eğitilenin adı veriyor, bunu düzelt! 
                if (ER == null)
                {
                    Eigen_label = "Tanımsız";
                    Eigen_Distance = 0;
                    return Eigen_label;
                }
                else
                {
                    Eigen_label = ER.Label;
                    Eigen_Distance = ER.Distance;
                    if (Eigen_Thresh > -1) Eigen_threshold = Eigen_Thresh;
                    if (Eigen_Distance > Eigen_threshold) return Eigen_label;
                    else return "Tanımsız";
                } 
            }
            else return "";
        }


        //NOT: Bu fonksiyon normalde güven aralığı hesaplaması lazım ama opencv update ile bir şekilde bozuldu. Kontrol et.!
        public int Set_Eigen_Threshold
        {
            set
            {
                Eigen_threshold = value;
            }
        }

       //Tanınan kişilerin adını aldırıyor
        public string Get_Eigen_Label
        {
            get
            {
                return Eigen_label;
            }
        }

        //Hata payını döndürür (Güven aralığı)
        public float Get_Eigen_Distance
        {
            get
            {
                 
                return Eigen_Distance;
            }
        }

        //Hatayı gösterir 
        public string Get_Error
        {
            get { return Error; }
        }

        //Eğitilenleri kayıt eder XML'e
        public void Save_Eigen_Recogniser(string filename)
        {
            StringBuilder sb = new StringBuilder();

            (new XmlSerializer(typeof(EigenObjectRecognizer))).Serialize(new StringWriter(sb), recognizer);
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(sb.ToString());
            xDoc.Save(filename);
        }

       
        //XML Dosyasını okutma  
        public void Load_Eigen_Recogniser(string filename)
        {
             
            FileStream EigenFS = File.OpenRead(filename);
            long Eflength = EigenFS.Length;
            byte[] xmlEBs = new byte[Eflength];
            EigenFS.Read(xmlEBs, 0, (int)Eflength);
            EigenFS.Close();

            MemoryStream xStream = new MemoryStream(xmlEBs);
            recognizer = (EigenObjectRecognizer)(new XmlSerializer(typeof(EigenObjectRecognizer))).Deserialize(xStream);
            _IsTrained = true;
             
        }

       
        public void Dispose()
        {
            recognizer = null;
            trainingImages = null;
            Names_List = null;
            Error = null;
            GC.Collect();
        }

        #endregion

        #region Private
        //Eğitim verilerini açma fonksiyonu
        private bool LoadTrainingData(string Folder_location)
        {
            if (File.Exists(Folder_location + "\\" + XmlVeriDosyasi))
            {
                try
                {
                     //XML Veri dosyasını açıp xmlBytes isimli diziye atıyoruz
                    Names_List.Clear();
                    trainingImages.Clear();
                    FileStream filestream = File.OpenRead(Folder_location + "\\" + XmlVeriDosyasi);
                    long filelength = filestream.Length;
                    byte[] xmlBytes = new byte[filelength];
                    filestream.Read(xmlBytes, 0, (int)filelength);
                    filestream.Close();

                    MemoryStream xmlStream = new MemoryStream(xmlBytes);

                    //Xmlreader kütüphanesi fonksiyonu ile program neye ihtiyaç duyarsa onu çekmesini sağlayan bir fonksiyon
                    using (XmlReader xmlreader = XmlTextReader.Create(xmlStream))
                    {
                        while (xmlreader.Read())
                        {
                            if (xmlreader.IsStartElement())
                            {
                                switch (xmlreader.Name)
                                {
                                    case "NAME":
                                        if (xmlreader.Read())
                                        {
                                            Names_List.Add(xmlreader.Value.Trim());
                                            NumLabels += 1;
                                        }
                                        break;
                                    case "FILE":
                                        if (xmlreader.Read())
                                        {
                                            
                                            trainingImages.Add(new Image<Gray, byte>(Dizin + "\\" + xmlreader.Value.Trim()));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    ContTrain = NumLabels;

                    if (trainingImages.ToArray().Length != 0)
                    {
                        //Eigen yüz tanıma fonksiyonuna eğitilmişleri aktardık ki bize yazdırsın
                        recognizer = new EigenObjectRecognizer(trainingImages.ToArray(),
                        Names_List.ToArray(), 5000, ref termCrit); //5000 default
                        return true;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    Error = ex.ToString();
                    return false;
                }
            }
            else return false;
        }

        #endregion
    }
}
