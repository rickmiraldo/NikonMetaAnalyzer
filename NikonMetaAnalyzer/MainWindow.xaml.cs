using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace NikonMetaAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<NikonPictureMeta> NikonPictures = new List<NikonPictureMeta>();
        List<TrajetoInstanteGPS> InstantesGPS = new List<TrajetoInstanteGPS>();
        List<NovoMeta> NovoArquivoMeta = new List<NovoMeta>();
        List<NovoEO> NovoArquivoEO = new List<NovoEO>();
        
        public class NikonPictureMeta
        {
            public string Date { get; set; }
            public string Pre_Time { get; set; }
            public string Pre_MS { get; set; }
            public string Post_Time { get; set; }
            public string Post_MS { get; set; }
            public string Record_Time { get; set; }
            public string Record_MS { get; set; }
            public string Filename { get; set; }
        }

        public class TrajetoInstanteGPS
        {
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string Altitude { get; set; }
        }

        public class NovoMeta
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string Altitude { get; set; }
            public string Filename { get; set; }
            public string TotalSeconds { get; set; }
            public string LatAnterior { get; set; }
            public string LongAnterior { get; set; }
            public string AltAnterior { get; set; }
            public string LatPosterior { get; set; }
            public string LongPosterior { get; set; }
            public string AltPosterior { get; set; }
        }

        public class NovoEO
        {
            public string foto { get; set; }
            public string este { get; set; }
            public string norte { get; set; }
            public string alt { get; set; }
            public string omega { get; set; }
            public string phi { get; set; }
            public string kappa { get; set; }
            public string gpsTime { get; set; }
        }
        
        public void gravaLog(string m)
        {
            File.AppendAllText("log.txt", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "]\t" + m + "\r\n");
            return;
        }
        
        public MainWindow()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); // Utilizar ponto como separador decimal ao invés de vírgula
            
            InitializeComponent();

            gravaLog("=======================");
            gravaLog("= NIKON META ANALYZER =\tVersão " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            gravaLog("=======================");
            gravaLog("Inicializando programa...");

            dgMetaNikon.ItemsSource = NikonPictures;
            dgTrajetoTxt.ItemsSource = InstantesGPS;
        }

        private void transformaGeodesicaTM(double latitude, double longitude, out double este, out double norte) // Transforma coordenadas geodésicas em planoretangulares TM
        {
            // Entrada em grau decimal
            
            // Constantes de projeção UTM (sul e oeste)
            double n0 = 10000000d;
            double e0 = 500000d;
            double k0 = 0.9996d;

            // Constantes datum WGS84
            double a = 6378137d;
            double b = 6356752.314d;
            //double f = 298.2572d;

            /* Constantes datum SAD69
            double a = 6378160d;
            double b = 6356774.719d;
            double f = 298.25d; */

            // Cálculos de primeira e segunda excentricidade
            double e2 = ((Math.Pow(a,2) - Math.Pow(b,2))/Math.Pow(a,2));
            double eLinha2 = ((Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(b, 2));
            double e = Math.Sqrt(e2);
            double eLinha = Math.Sqrt(eLinha2);

            // Constantes elipsoidais
            double ro0 = 180d / Math.PI;
            double constElipA = (1d + (3d / 4d * e2) + (45d / 64d * e2 * e2) + (175d / 256d * e2 * e2 * e2) + (11025d / 16384d * e2 * e2 * e2 * e2) + (43659d / 65536d * e2 * e2 * e2 * e2 * e2));
            double constElipB = (3d / 4d * e2) + (15d / 16d * e2 * e2) + (525d / 512d * e2 * e2 * e2) + (2205d / 2048d * e2 * e2 * e2 * e2) + (72765d / 65536d * e2 * e2 * e2 * e2 * e2);
            double constElipC = (15d / 64d * e2 * e2) + (105d / 256d * e2 * e2 * e2) + (2205d / 4096d * e2 * e2 * e2 * e2) + (10395d / 16384d * e2 * e2 * e2 * e2 * e2);
            double constElipD = (35d / 512d * e2 * e2 * e2) + (315d / 2048d * e2 * e2 * e2 * e2) + (31185d / 131072d * e2 * e2 * e2 * e2 * e2);
            double constElipE = (315d / 16384d * e2 * e2 * e2 * e2) + (3465d / 65536d * e2 * e2 * e2 * e2 * e2);
            double constElipF = (639d / 131072d * e2 * e2 * e2 * e2 * e2);

            double alfa = (constElipA * a * (1d - e2)) / ro0;
            double beta = (constElipB * a * (1d - e2)) / 2d;
            double gamma = (constElipC * a * (1d - e2)) / 4d;
            double delta = (constElipD * a * (1d - e2)) / 6d;
            double epsilon = (constElipE * a * (1d - e2)) / 8d;
            double csi = (constElipF * a * (1d - e2)) / 10d;

            // Cálculo meridiano central
            double m0 = ((((int)Math.Truncate(Math.Abs(longitude) / 6d)) * 6d) + 3d);
            if (longitude < 0) m0 *= -1;
            
            // Conversão
            double latRad = latitude / ro0;
            double deltaLambdaSeg = (longitude - m0) * 3600d;
            double bzao = (alfa * latRad * ro0) - (beta * Math.Sin(2 * latRad)) + (gamma * Math.Sin(4 * latRad)) - (delta * Math.Sin(6 * latRad)) + (epsilon * Math.Sin(8 * latRad)) - (csi * Math.Sin(10 * latRad));
            double normal = a / (Math.Sqrt(1 - e2 * Math.Sin(latRad) * Math.Sin(latRad)));
            double t = Math.Tan(latRad);
            double etha = eLinha * Math.Cos(latRad);

            double t2 = Math.Pow(t, 2);
            double etha2 = Math.Pow(etha, 2);
            double senoLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double sen1s = Math.Sin((1d / 3600d) * ((Math.Atan(1)) / 45d));

            // Cálculo do Norte e Este
            double norteLinha = k0 * bzao + k0 * (Math.Pow(deltaLambdaSeg, 2) * (1d / 2d * normal * senoLat * cosLat * Math.Pow(sen1s, 2)) +
                            Math.Pow(deltaLambdaSeg, 4) * (1d / 24d * normal * senoLat * Math.Pow(cosLat, 3) * Math.Pow(sen1s, 4)) * (5d - t2 + 9d * etha2 + 4d * etha2 * etha2) +
                            Math.Pow(deltaLambdaSeg, 6) * (1d / 720d * normal * senoLat * Math.Pow(cosLat, 5) * Math.Pow(sen1s, 6)) * (61d - 58d * t2 + 720d * etha2 - 350d * t2 * etha2));
            double esteLinha = k0 * (deltaLambdaSeg * normal * cosLat * sen1s +
                            Math.Pow(deltaLambdaSeg, 3) * (1d / 6d * normal * Math.Pow(cosLat, 3) * Math.Pow(sen1s, 3)) * (1d - t2 + etha2) +
                            Math.Pow(deltaLambdaSeg, 5) * (1d / 120d * normal * Math.Pow(cosLat, 5) * Math.Pow(sen1s, 5) * (5d - 18d * t2 + t2 * t2 + 14d * etha2 - 58d * t2 * etha2)));

            norte = n0 + norteLinha;
            este = e0 + esteLinha;

            // Cálculo de coeficiente de deformação linear
            double meridiana = (a * (1d - e2))/(Math.Sqrt(Math.Pow(1d - e2 * senoLat * senoLat, 3)));
            double raioMedio2 = meridiana * normal;
            double k = k0 * (1d + (Math.Pow(esteLinha, 2) / (2d * raioMedio2)) + (Math.Pow(esteLinha, 4) / (24d * raioMedio2 * raioMedio2)));

            // Cálculo da convergência meridiana
            double convergMeridianaSeg = deltaLambdaSeg * senoLat + ((senoLat * cosLat * cosLat * sen1s * sen1s) / 3d) *
                                              (1 + 3d * etha2 + 2d * etha2 * etha2) * Math.Pow(deltaLambdaSeg, 3) +
                                              ((Math.Pow(sen1s, 4)) / 15d) * senoLat * Math.Pow(cosLat, 4) * (2d - t2) * Math.Pow(deltaLambdaSeg, 5);
        }

        private void miCarregarMetaNikon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Nikon Meta (*.txt)|*.txt";
            //d.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            d.ShowDialog();

            if (d.FileName != "")
            {
                string arqNikonMeta = d.FileName;
                gravaLog("Abrindo arquivo Meta Nikon: " + arqNikonMeta);
                NikonPictures.Clear();

                using (var arquivo = new StreamReader(arqNikonMeta)) // Carrega arquivo
                {
                    string linha;
                    int contOK = 0;
                    int contINV = 0;
                    int contBRA = 0;

                    while ((linha = arquivo.ReadLine()) != null) // Lê linha por linha
                    {
                        if (linha.Length > 0)
                        {
                            char[] validChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }; // Caracteres válidos para início de linha do Nikon Meta

                            if (validChars.Contains(linha[0]))
                            {
                                string[] split = linha.Split('\t');

                                NikonPictures.Add(new NikonPictureMeta() { Date = split[0], Pre_Time = split[1], Pre_MS = split[2], Post_Time = split[3], Post_MS = split[4], Record_Time = split[5], Record_MS = split[6], Filename = split[7] });
                                contOK++;
                            }
                            else
                            {
                                contINV++;
                            }
                        }
                        else
                        {
                            contBRA++;
                        }
                    }
                    gravaLog("Arquivo Meta Nikon lido! Linhas válidas: " + contOK + " / Em branco: " + contBRA + " / Inválidas: " + contINV);
                    tbStatusText.Text = "Arquivo Meta Nikon lido: " + contOK + " linha(s) válidas.";

                    dgMetaNikon.Items.Refresh();
                }
            }
        }

        private void miCarregarTrajetoTxt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Trajeto GPS (*.txt)|*.txt";
            //d.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            d.ShowDialog();

            if (d.FileName != "")
            {
                string arqTrajGPS = d.FileName;
                gravaLog("Abrindo arquivo Trajeto GPS: " + arqTrajGPS);
                InstantesGPS.Clear();

                using (var arquivo = new StreamReader(arqTrajGPS)) // Carrega arquivo
                {
                    string linha;
                    int contOK = 0;
                    int contINV = 0;
                    int contBRA = 0;

                    while ((linha = arquivo.ReadLine()) != null) // Lê linha por linha
                    {
                        if (linha.Length > 0)
                        {
                            if (linha.Contains("t,d,"))
                            {
                                string[] split = linha.Split(',');

                                if (split[4].Equals("00/00/00"))
                                {
                                    contINV++;
                                }
                                else
                                {
                                    contOK++;
                                    InstantesGPS.Add(new TrajetoInstanteGPS() { Latitude = split[2], Longitude = split[3], Date = split[4], Time = split[5], Altitude = split[6] });
                                }
                            }
                            else
                            {
                                contINV++;
                            }
                        }
                        else
                        {
                            contBRA++;
                        }
                    }
                    gravaLog("Arquivo Trajeto GPS lido! Linhas válidas: " + contOK + " / Em branco: " + contBRA + " / Inválidas: " + contINV);
                    tbStatusText.Text = "Arquivo Trajeto GPS lido: " + contOK + " linha(s) válidas.";

                    dgTrajetoTxt.Items.Refresh();
                }
            }
        }

        private void btAplicarOffsetTrajeto_Click(object sender, RoutedEventArgs e)
        {
            if (tbOffsetHoras.Text == "") tbOffsetHoras.Text = "0";
            if (tbOffsetMinutos.Text == "") tbOffsetMinutos.Text = "0";
            if (tbOffsetSegundos.Text == "") tbOffsetSegundos.Text = "0";
            
            if (InstantesGPS.Count == 0) {
                System.Windows.MessageBox.Show("Não há informações de Trajeto GPS. Carregue um arquivo antes.", "Erro");
                return;
            }
            else
            {
                try
                {
                    if ((Convert.ToInt32(tbOffsetMinutos.Text) >= 60) || (Convert.ToInt32(tbOffsetMinutos.Text) < 0) || (Convert.ToInt32(tbOffsetSegundos.Text) >= 60) || (Convert.ToInt32(tbOffsetSegundos.Text) < 0) || (Convert.ToInt32(tbOffsetHoras.Text) < 0))
                    {
                        System.Windows.MessageBox.Show("Valores inválidos", "Erro");
                        return;
                    }
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Valores não são numéricos", "Erro");
                    return;
                }
                
                InstantesGPS.ForEach(delegate(TrajetoInstanteGPS i)
                {
                    string s = i.Time;

                    int hora = Convert.ToInt32(s.Substring(0, 2));
                    int minuto = Convert.ToInt32(s.Substring(3, 2));
                    int segundo = Convert.ToInt32(s.Substring(6, 2));

                    int horaOffset = Convert.ToInt32(tbOffsetHoras.Text);
                    int minutoOffset = Convert.ToInt32(tbOffsetMinutos.Text);
                    int segundoOffset = Convert.ToInt32(tbOffsetSegundos.Text);

                    if ((bool)rbAddOffset.IsChecked)
                    {
                        // Soma offset
                        segundo += segundoOffset;
                        if (segundo >= 60)
                        {
                            segundo -= 60;
                            minuto++;
                        }

                        minuto += minutoOffset;
                        if (minuto >= 60)
                        {
                            minuto -= 60;
                            hora++;
                        }

                        hora += horaOffset;
                    }
                    else
                    {
                        // Subtrai offset
                        segundo -= segundoOffset;
                        if (segundo < 0)
                        {
                            segundo += 60;
                            minuto--;
                        }

                        minuto -= minutoOffset;
                        if (minuto < 0)
                        {
                            minuto += 60;
                            hora--;
                        }

                        hora -= horaOffset;
                    }

                    // Adiciona "0" na casa da dezena, caso não tenha. O arquivo ficará errado se não fizer isso
                    string newHora = hora >= 10 ? Convert.ToString(hora) : "0" + hora;
                    string newMinuto = minuto >= 10 ? Convert.ToString(minuto) : "0" + minuto;
                    string newSegundo = segundo >= 10 ? Convert.ToString(segundo) : "0" + segundo;

                    s = newHora + ":" + newMinuto + ":" + newSegundo;

                    i.Time = s;
                });

                dgTrajetoTxt.Items.Refresh();

                // Adiciona "0" na casa da dezena, caso não tenha. Este processo é apenas estético
                string l1 = tbOffsetHoras.Text;
                string l2 = tbOffsetMinutos.Text.Length == 1 ? "0" + tbOffsetMinutos.Text : tbOffsetMinutos.Text;
                string l3 = tbOffsetSegundos.Text.Length == 1 ? "0" + tbOffsetSegundos.Text : tbOffsetSegundos.Text;

                string l = l1 + ":" + l2 + ":" + l3;
                if ((bool)rbAddOffset.IsChecked) l += " adicionado";
                else l += " subtraído";
                gravaLog("Offset de " + l + " do Trajeto GPS.");
                tbStatusText.Text = "Offset de " + l + ".";
            }
            
            
        }

        private void miSair_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void miSobre_Click(object sender, RoutedEventArgs e)
        {
            string v = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string m = ("Nikon Meta Analyzer " + v + "\nBASE Aerofotogrametria e Projetos S.A.\n\nProgramação: Henrique Germano Miraldo");

            System.Windows.MessageBox.Show(m, "Sobre");
        }

        private void btGerarNovoMeta_Click(object sender, RoutedEventArgs e)
        {
            if ((NikonPictures.Count == 0) || (InstantesGPS.Count == 0))
            {
                System.Windows.MessageBox.Show("Carregue as duas listas acima antes de gerar os novos arquivos.", "Erro");
                return;
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Arquivo de texto (*.txt)|*.txt";
                dialog.ShowDialog();

                if (dialog.FileName == "") return;

                string pathToMeta = dialog.FileName;
                string pathToKML = pathToMeta.Replace(".txt", ".kml");
                string pathToEo = pathToMeta.Replace(".txt", ".eo");

                NovoArquivoMeta.Clear();
                
                // Para cada item da lista do Meta Nikon, fazer...
                for (int i = 0; i < NikonPictures.Count; i++)
                {
                    NovoMeta newMeta = new NovoMeta();
                    tbStatusText.Text = "Trabalhando...";
                    
                    // Cálculo do instante da captura

                    string timeString = NikonPictures[i].Pre_Time;
                    int hora = Convert.ToInt32(timeString.Substring(0, 2));
                    int minuto = Convert.ToInt32(timeString.Substring(2, 2));
                    int segundo = Convert.ToInt32(timeString.Substring(4, 2));
                    double ms = Convert.ToDouble(NikonPictures[i].Pre_MS) / 10000000;/////
                    double preTime = segundo + (minuto * 60) + (hora * 3600) + ms; // Valor em segundos/milisegundos do PreTime

                    timeString = NikonPictures[i].Post_Time;/////
                    hora = Convert.ToInt32(timeString.Substring(0, 2));
                    minuto = Convert.ToInt32(timeString.Substring(2, 2));
                    segundo = Convert.ToInt32(timeString.Substring(4, 2));
                    ms = Convert.ToDouble(NikonPictures[i].Post_MS) / 10000000;/////
                    double posTime = segundo + (minuto * 60) + (hora * 3600) + ms; // Valor em segundos/milisegundos do PosTime

                    double captureTime = (preTime + posTime) / 2; // Instante da captura em segundos/milisegundos

                    int captureTimeInteiro = (int)Math.Truncate(captureTime); // Separar o instante da captura em hh:mm:ss.fffffff
                    hora = captureTimeInteiro / 3600;
                    minuto = (captureTimeInteiro % 3600) / 60;
                    segundo = (captureTimeInteiro % 3600) % 60;
                    ms = captureTime - captureTimeInteiro;
                    string msString = Convert.ToString(ms);

                    string horaString = hora >= 10 ? Convert.ToString(hora) : "0" + hora; // Coloca 0 nas strings, se necessário
                    string minutoString = minuto >= 10 ? Convert.ToString(minuto) : "0" + minuto;
                    string segundoString = segundo >= 10 ? Convert.ToString(segundo) : "0" + segundo;

                    timeString = horaString + ":" + minutoString + ":" + segundoString + "." + msString.Substring(2, 2); // Milisegundos com 2 casas de precisão

                    newMeta.Time = timeString;

                    // Cálculo do Lat/Long/Alt

                    for (int j = 1; j < InstantesGPS.Count - 1; j++) // Varre a lista de instantes GPS
                    {
                        timeString = InstantesGPS[j].Time; // Separa o instante em hh:mm:ss
                        hora = Convert.ToInt32(timeString.Substring(0, 2));
                        minuto = Convert.ToInt32(timeString.Substring(3, 2));
                        segundo = Convert.ToInt32(timeString.Substring(6, 2));

                        double gpsTime = segundo + (minuto * 60) + (hora * 3600); // Instante em segundos da entrada atual

                        if (captureTime < gpsTime)
                        {
                            timeString = InstantesGPS[j - 1].Time; // Separa o instante anterior em hh:mm:ss
                            hora = Convert.ToInt32(timeString.Substring(0, 2));
                            minuto = Convert.ToInt32(timeString.Substring(3, 2));
                            segundo = Convert.ToInt32(timeString.Substring(6, 2));

                            double gpsTimeAnterior = segundo + (minuto * 60) + (hora * 3600); // Instante em segundos da entrada anterior

                            double latAtual = Convert.ToDouble(InstantesGPS[j].Latitude);
                            double longAtual = Convert.ToDouble(InstantesGPS[j].Longitude);
                            double altAtual = Convert.ToDouble(InstantesGPS[j].Altitude);

                            double latAnterior = Convert.ToDouble(InstantesGPS[j - 1].Latitude);
                            double longAnterior = Convert.ToDouble(InstantesGPS[j - 1].Longitude);
                            double altAnterior = Convert.ToDouble(InstantesGPS[j - 1].Altitude);

                            double deltaLat = latAtual - latAnterior; // Diferença entra a posição atual e anterior do registro GPS
                            double deltaLong = longAtual - longAnterior;
                            double deltaAlt = altAtual - altAnterior;

                            double diferencaInstante = captureTime - gpsTimeAnterior; // Diferença entre o instante do registro anterior do GPS e do instante de captura da foto

                            double newLat = latAnterior + (deltaLat * diferencaInstante); // Cálculo da diferença entre a posição do registro anterior do GPS e do instante
                            double newLong = longAnterior + (deltaLong * diferencaInstante); // de captura da foto. O resultado já é somado ao valor da posição do instante
                            double newAlt = altAnterior + (deltaAlt * diferencaInstante); // anterior, formando assim a posição do instante da tomada da foto

                            string newLatString = Convert.ToString((Math.Truncate(newLat * 1000000) / 1000000)); // Truncando valores na 6a. casa decimal
                            string newLongString = Convert.ToString((Math.Truncate(newLong * 1000000) / 1000000));
                            string newAltString = Convert.ToString((Math.Truncate(newAlt * 100) / 100));

                            string latAnteriorString = Convert.ToString((Math.Truncate(latAnterior * 1000000) / 1000000));
                            string longAnteriorString = Convert.ToString((Math.Truncate(longAnterior * 1000000) / 1000000));
                            string altAnteriorString = Convert.ToString((Math.Truncate(altAnterior * 100) / 100));

                            double latPosterior = Convert.ToDouble(InstantesGPS[j + 1].Latitude);
                            double longPosterior = Convert.ToDouble(InstantesGPS[j + 1].Longitude);
                            double altPosterior = Convert.ToDouble(InstantesGPS[j + 1].Altitude);

                            string latPosteriorString = Convert.ToString((Math.Truncate(latPosterior * 1000000) / 1000000));
                            string longPosteriorString = Convert.ToString((Math.Truncate(longPosterior * 1000000) / 1000000));
                            string altPosteriorString = Convert.ToString((Math.Truncate(altPosterior * 100) / 100));

                            newMeta.Latitude = newLatString;
                            newMeta.Longitude = newLongString;
                            newMeta.Altitude = newAltString;

                            newMeta.LatAnterior = latAnteriorString;
                            newMeta.LongAnterior = longAnteriorString;
                            newMeta.AltAnterior = altAnteriorString;

                            newMeta.LatPosterior = latPosteriorString;
                            newMeta.LongPosterior = longPosteriorString;
                            newMeta.AltPosterior = altPosteriorString;

                            break;
                        }
                        
                    }

                    // Outras informações

                    newMeta.Date = NikonPictures[i].Date;
                    newMeta.Filename = NikonPictures[i].Filename;
                    newMeta.TotalSeconds = Convert.ToString(captureTime);

                    // Adiciona novo meta criado à lista
                    NovoArquivoMeta.Add(newMeta);
                }

                // Salva arquivo Meta

                using (var arquivoMeta = new StreamWriter(pathToMeta))
                {
                    arquivoMeta.WriteLine("DATA\tHORA\tLAT\tLONG\tALT\tNOME_ARQUIVO\tTOTAL_SEG\tLAT_ANT\tLONG_ANT\tALT_ANT\tLAT_POS\tLONG_POS\tALT_POS");
                    for (int i = 0; i < NovoArquivoMeta.Count; i++)
                    {
                        string linha = //i + "\t" +
                                       NovoArquivoMeta[i].Date + "\t" +
                                       NovoArquivoMeta[i].Time + "\t" +
                                       NovoArquivoMeta[i].Latitude + "\t" +
                                       NovoArquivoMeta[i].Longitude + "\t" +
                                       NovoArquivoMeta[i].Altitude + "\t" +
                                       NovoArquivoMeta[i].Filename + "\t" +
                                       NovoArquivoMeta[i].TotalSeconds + "\t" +
                                       NovoArquivoMeta[i].LatAnterior + "\t" +
                                       NovoArquivoMeta[i].LongAnterior + "\t" +
                                       NovoArquivoMeta[i].AltAnterior + "\t" +
                                       NovoArquivoMeta[i].LatPosterior + "\t" +
                                       NovoArquivoMeta[i].LongPosterior + "\t" +
                                       NovoArquivoMeta[i].AltPosterior;
                        arquivoMeta.WriteLine(linha);
                    }
                }

                // Salva arquivo KML
                
                using (var arquivoKML = new StreamWriter(pathToKML))
                {
                    arquivoKML.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<kml xmlns=\"http://www.opengis.net/kml/2.2\">\r\n<Document>\r\n" +
                                         "<Style id=\"icone\">\r\n<IconStyle>\r\n<Icon>\r\n<href>http://maps.google.com/mapfiles/kml/shapes/placemark_square.png</href>" +
                                         "\r\n</Icon>\r\n</IconStyle>\r\n</Style>\r\n<Style id=\"linhaAmarela\">\r\n<LineStyle><color>ff00ffff</color>\r\n" +
                                         "<width>3</width>\r\n</LineStyle>\r\n</Style>\r\n<Folder>\r\n<name>Imagens do voo</name>");
                    for (int i = 0; i < NovoArquivoMeta.Count; i++)
                    {
                        string linha = "<Placemark>\r\n" +
                                       "<name>" + NovoArquivoMeta[i].Time.Substring(0, 8) + "</name>\r\n" +
                                       "<styleUrl>#icone</styleUrl>\r\n<description><![CDATA[Nome do arquivo: <a href=\"Imagens\\" +
                                       NovoArquivoMeta[i].Filename + "\">" + NovoArquivoMeta[i].Filename + "</a><br>Altitude: " +
                                       NovoArquivoMeta[i].Altitude + "]]></description>\r\n<Point>\r\n<coordinates>" +
                                       NovoArquivoMeta[i].Longitude + "," + NovoArquivoMeta[i].Latitude + ",0</coordinates>\r\n</Point>\r\n" +
                                       "</Placemark>";
                        arquivoKML.WriteLine(linha);
                    }

                    arquivoKML.WriteLine("</Folder>\r\n<Folder>\r\n<name>Trajeto do helicóptero</name>\r\n<Placemark>\r\n" +
                                         "<name>Trajeto</name>\r\n<description>Trajeto percorrido pelo helicóptero durante a tomada de fotos</description>\r\n" +
                                         "<styleUrl>#linhaAmarela</styleUrl>\r\n<LineString>\r\n<coordinates>");

                    for (int i = 0; i < InstantesGPS.Count; i++)
                    {
                        string linha = InstantesGPS[i].Longitude + "," + InstantesGPS[i].Latitude + ",0";
                        arquivoKML.WriteLine(linha);
                    }
                    arquivoKML.WriteLine("</coordinates>\r\n</LineString>\r\n</Placemark>\r\n</Folder>\r\n</Document>\r\n</kml>");
                }
                
                // Gera arquivo EO

                NovoArquivoEO.Clear();

                for (int i = 0; i < NovoArquivoMeta.Count; i++)
                {
                    NovoEO eo = new NovoEO();

                    double esteAnterior;
                    double norteAnterior;
                    double estePosterior;
                    double nortePosterior;

                    double kappa;
    
                    double latAnterior = Convert.ToDouble(NovoArquivoMeta[i].LatAnterior);
                    double longAnterior = Convert.ToDouble(NovoArquivoMeta[i].LongAnterior);
                    double latPosterior = Convert.ToDouble(NovoArquivoMeta[i].LatPosterior);
                    double longPosterior = Convert.ToDouble(NovoArquivoMeta[i].LongPosterior);

                    transformaGeodesicaTM(latAnterior, longAnterior, out esteAnterior, out norteAnterior);
                    transformaGeodesicaTM(latPosterior, longPosterior, out estePosterior, out nortePosterior);

                    double deltaEste = estePosterior - esteAnterior;
                    double deltaNorte = nortePosterior - norteAnterior;

                    if ((deltaEste == 0) && (deltaEste == 0))
                    {
                        kappa = 0d;
                    } else if ((deltaEste == 0) && (deltaNorte > 0))
                    {
                        kappa = Math.PI / 2d;
                    } else if ((deltaEste == 0) && (deltaNorte < 0))
                    {
                        kappa = 3d * Math.PI / 2d;
                    } else if ((deltaEste > 0) && (deltaNorte >= 0))
                    {
                        kappa = Math.Atan(deltaNorte/deltaEste);
                    } else if ((deltaEste > 0) && (deltaNorte < 0))
                    {
                        kappa = 2d * Math.PI + Math.Atan(deltaNorte/deltaEste);
                    } else 
                    {
                        kappa = Math.PI + Math.Atan(deltaNorte/deltaEste);
                    }

                    double kappaGrado = kappa * 200d / Math.PI;
    
                    eo.kappa = Convert.ToString((Math.Truncate(kappaGrado * 10000) / 10000));
    
                    double latAtual = Convert.ToDouble(NovoArquivoMeta[i].Latitude);
                    double longAtual = Convert.ToDouble(NovoArquivoMeta[i].Longitude);;
    
                    double esteAtual;
                    double norteAtual;
    
                    transformaGeodesicaTM(latAtual, longAtual, out esteAtual, out norteAtual);
    
                    string esteAtualStr = Convert.ToString((Math.Truncate(esteAtual * 1000) / 1000));
                    string norteAtualStr = Convert.ToString((Math.Truncate(norteAtual * 1000) / 1000));

                    eo.norte = norteAtualStr;
                    eo.este = esteAtualStr;
    
                    eo.alt = NovoArquivoMeta[i].Altitude;
                    eo.gpsTime = NovoArquivoMeta[i].TotalSeconds;
                    eo.omega = Convert.ToString(0);
                    eo.phi = Convert.ToString(0);
    
                    string foto = NovoArquivoMeta[i].Filename;
                    int index = foto.IndexOf(".");
                    eo.foto = foto.Substring(0, index);

                    Console.WriteLine(eo.foto + "    " +
                                      eo.este + "    " +
                                      eo.norte + "    " +
                                      eo.alt + "    " +
                                      eo.omega + "    " +
                                      eo.phi + "    " +
                                      eo.kappa + "    " +
                                      eo.gpsTime);
    
                    NovoArquivoEO.Add(eo);
                }

                using (var arquivoEo = new StreamWriter(pathToEo))
                {
                    arquivoEo.WriteLine("Distance: meters, Angles: Centesimal\r\nPhotoID       East     North      Height      Omega     Phi     Kappa     GPS Time\r\n-gpsins     1     1");
                    for (int i = 0; i < NovoArquivoEO.Count; i++)
                    {
                        string linha = NovoArquivoEO[i].foto + "    " +
                                       NovoArquivoEO[i].este + "    " +
                                       NovoArquivoEO[i].norte + "    " +
                                       NovoArquivoEO[i].alt + "    " +
                                       NovoArquivoEO[i].omega + "    " +
                                       NovoArquivoEO[i].phi + "    " +
                                       NovoArquivoEO[i].kappa + "    " +
                                       NovoArquivoEO[i].gpsTime + "    1    1";
                        arquivoEo.WriteLine(linha);
                    }
                }

                gravaLog("Novos arquivos gerados com " + NovoArquivoMeta.Count + " registros.");
                gravaLog("Arquivo Meta salvo em: " + pathToMeta);
                gravaLog("Arquivo KML salvo em: " + pathToKML);
                gravaLog("Arquivo EO salvo em: " + pathToEo);
                tbStatusText.Text = "Novos arquivos gerados com " + NovoArquivoMeta.Count + " registros.";
                System.Windows.MessageBox.Show("Processo concluído com sucesso!", "Pronto");

            }
        }
        
        private void wiMain_Closing(object sender, CancelEventArgs e)
        {
            gravaLog("Saindo...\r\n");
        }

    }
}
