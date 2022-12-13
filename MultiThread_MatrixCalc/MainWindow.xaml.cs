//Multithread MatrixCalc v1.0 (c) Kabluchkov D.S.
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
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace MultiThread_MatrixCalc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
       

        int n = 0;
        int[,] A, B, C, Cmulti, Cparallel;
        int angle = 0;
        RotateTransform rt;
        string matrix1Viz, matrix2Viz, matrix3Viz, regTime, multiTime, parallelTime, regtextSum, multitextSum, paralleltextSum, uiText, backgroundText;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        int threadsCount;

        //метод многопоточного умножения
        void ThreadMultiply(object o)
        {
            int start = ((int[])o)[0];
            int end = ((int[])o)[1];
            for (int i = start; i < end; i++)
            {
                for (int j = 0; j<n; j++)
                {
                    for (int k = 0; k<n; k++)
                    {
                        Cmulti[i, j] = A[i, k] * B[k, j];
                    }
                }
            }
        }

        

        public MainWindow()
        {
            InitializeComponent();
            //подписываем фоновый обработчик на методы
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            //таймер, отвечающий за крутку текста
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
        }


        //метод расчёта трёх матриц
        void MatrixCalc()
        {
            //объект рандомайзера
            Random rnd = new Random();

            //объект таймера
            Stopwatch sw = new Stopwatch();



            //генерируем 1ю матрицу
            A = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) A[i, j] = rnd.Next(10);
            }

            //визуальный контроль 1й матрицы
            string visualControl = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) visualControl += Convert.ToString(A[i, j]) + "-";
            }
            //выгружаем результат наружу
            matrix1Viz = visualControl;

            //генерируем 2ю матрицу
            B = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) B[i, j] = rnd.Next(10);
            }

            //визуальный контроль 2й матрицы
            visualControl = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) visualControl += Convert.ToString(B[i, j]) + "-";
            }
            //выгружаем результат наружу
            matrix2Viz = visualControl;

            //==========================================  ОБЫЧНЫЙ  ====================================

            //создаём 3ю матрицу регулярным методом
            C = new int[n, n];
            //пуск таймера
            sw.Start();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        C[i, j] = A[i, k] * B[k, j];
                    }
                }
            }
            //стоп таймера и вывод
            sw.Stop();

            //выгружаем время наружу
            regTime = Convert.ToString(sw.ElapsedMilliseconds);

            //визуальный контроль сгенерированной регуляркой 3й матрицы
            visualControl = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) visualControl += Convert.ToString(C[i, j]) + "-";
            }
            //выгружаем результат наружу
            matrix3Viz = visualControl;

            //контрольная сумма регулярной матрицы
            int regularSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) regularSum += C[i, j];
            }
            //выгружаем результат наружу
            regtextSum = Convert.ToString(regularSum);

            //=========================  МНОГОПОТОЧНЫЙ  =====================================================
            //создаём 3ю матрицу многопоточным методом
            //инициализируем матрицу
            Cmulti = new int[n, n];

            //создаём массив потоков
            Thread[] threads = new Thread[threadsCount];

            //запуск потоков
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread(ThreadMultiply);
            }

            //пуск таймера
            sw.Restart();

            //подсчёт старт-стопов========================
            int[] start = new int[threadsCount];
            int[] end = new int[threadsCount];

            //количествострок на потоки кроме последнего
            int rowsPerThread = n / threadsCount;

            //остаток строк на последний поток
            int lastrowsPerThread = n % threadsCount;

            int rowCounter = 0;

            for (int i = 0; i < threadsCount; i++)
            {
                //если итерация не последняя
                if (i != threadsCount - 1)
                {
                    start[i] = rowCounter;
                    end[i] = rowCounter + rowsPerThread + 1;
                }
                //если итерация последняя
                else
                {
                    start[i] = rowCounter;
                    end[i] = n;
                }
                //наращиваем сдвиг строк
                rowCounter += rowsPerThread;
            }
            //конец подсчёта старт-стопов=================


            //запуск расчётов в потоках
            for (int i = 0; i < threadsCount; i++)
            {
                object startend = new int[] { start[i], end[i] };
                threads[i].Start(startend);
            }

            //ожидание завершения потоков
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Join();
            }

            //стоп таймера и вывод
            sw.Stop();
            //выгружаем время наружу
            multiTime = Convert.ToString(sw.ElapsedMilliseconds);

            //контрольная сумма многопоточной матрицы
            int multiSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) multiSum += Cmulti[i, j];
            }
            //выгружаем результат наружу
            multitextSum = Convert.ToString(multiSum);


            //=================================  ПАРАЛЛЕЛЬНЫЙ  ================================================ 
            //создаём 3ю матрицу многопоточным методом
            //инициализируем матрицу
            Cparallel = new int[n, n];

            //пуск таймера
            sw.Restart();

            Parallel.For(0, n, i =>
            {

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        Cparallel[i, j] = A[i, k] * B[k, j];
                    }
                }

            });

            //стоп таймера и вывод
            sw.Stop();
            //выгружаем время наружу
            parallelTime = Convert.ToString(sw.ElapsedMilliseconds);

            //контрольная сумма параллельной матрицы
            int parallelSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) parallelSum += Cparallel[i, j];
            }
            //выгружаем результат наружу
            paralleltextSum = Convert.ToString(parallelSum);
        }

        //работаем в фоне
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //вычисляем номер текущего потока
            backgroundText = Convert.ToString(Thread.CurrentThread.ManagedThreadId);
            //считаем
            MatrixCalc();
        }
      
        //закончили работать в фоне
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            matrix1Visual.Text = matrix1Viz;
            matrix2Visual.Text = matrix2Viz;
            matrix3Visual.Text = matrix3Viz;

            regularTimeBox.Text = regTime;
            multiTimeBox.Text = multiTime;
            parallelTimeBox.Text = parallelTime;

            regularSumBox.Text = regtextSum;
            multiSumBox.Text = multitextSum;
            parallelSumBox.Text = paralleltextSum;

            //вывод информации по потокам
            uiThreadNumber.Text = uiText;
            backgroundThreadNumber.Text=backgroundText;
        }

        //обработка тика таймера
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //крутим что б видеть что приложение живое
            if (angle >= 360) angle = 0; 
            rt = new RotateTransform() { Angle = angle+=5};
            liveMeter.LayoutTransform = rt;
        }


        //кнопка запуска расчёта
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //обозначаем размер матриц
            n = Convert.ToInt32(nMatrixSize.Text);

            //получаем количество потоков
            threadsCount = Convert.ToInt32(threadsBox.Text);

            //вычисляем номер текущего потока
            uiText = Convert.ToString(Thread.CurrentThread.ManagedThreadId);

            //запуск фонового обработчика
            worker.RunWorkerAsync();
        }
    }
}
