using NAudio.Wave;
using OxyPlot.Series;
using OxyPlot;
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
using Microsoft.Win32;
using MathNet.Numerics.IntegralTransforms;

namespace SoundAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Otwórz okno dialogowe wyboru pliku
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files (*.wav)|*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                // Wczytaj plik .wav
                string filePath = openFileDialog.FileName;

                ProcessAudioFile(filePath);
            }
        }

        private void ProcessAudioFile(string filePath)
        {
            // Wczytaj dane audio z pliku
            using (var reader = new WaveFileReader(filePath))
            {
                var sampleRate = reader.WaveFormat.SampleRate;
                int totalSamples = (int)(reader.Length / 2); 
                var byteBuffer = new byte[totalSamples * 2];  
                var floatBuffer = new float[totalSamples];    

                // Odczyt danych jako byte[]
                int bytesRead = reader.Read(byteBuffer, 0, byteBuffer.Length);

                // Konwersja z byte[] na float[]
                for (int i = 0; i < bytesRead / 2; i++)  
                {
                    short sample = BitConverter.ToInt16(byteBuffer, i * 2);  
                    floatBuffer[i] = sample / 32768f;  
                }

                var fftResult = GetFFT(floatBuffer);

                // Inicjalizowanie wykresu OxyPlot (Biblioteka)
                var plotModel = new PlotModel { Title = "Audio Spectrum" };
                var series = new LineSeries
                {
                    Color = OxyColors.Blue,
                    StrokeThickness = 1,
                    MarkerSize = 3
                };

                // Dodawanie punktów do wykresu
                for (int i = 0; i < fftResult.Length; i++)
                {
                    series.Points.Add(new DataPoint(i * sampleRate / floatBuffer.Length, fftResult[i]));
                }

                plotModel.Series.Add(series);

                // Wyświetlenie wykresu
                plotView.Model = plotModel;
            }
        }



        private double[] GetFFT(float[] audioData)
        {
            int n = audioData.Length;
            var fftResult = new double[n / 2];

            // Przygotowanie danych dla FFT - muszą być typu Complex
            var complexData = new System.Numerics.Complex[n];
            for (int i = 0; i < n; i++)
            {
                complexData[i] = new System.Numerics.Complex(audioData[i], 0); // Zamiana na liczby zespolone
            }

            // Zastosowanie FFT
            Fourier.Forward(complexData, FourierOptions.Matlab);

            // Przekształcenie wyników FFT na amplitudy
            for (int i = 0; i < n / 2; i++)
            {
                fftResult[i] = complexData[i].Magnitude;
            }

            return fftResult;
        }
    }
}
