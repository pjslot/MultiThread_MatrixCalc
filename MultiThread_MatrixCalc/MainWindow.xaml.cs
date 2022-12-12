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
            //таймер, отвечающий за крутку текста
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
        }

        //обработка тика таймера
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //крутим что б видеть что приложение живое
            if (angle >= 360) angle = 0; 
            rt = new RotateTransform() { Angle = angle+=5};
            liveMeter.LayoutTransform = rt;

        }

        //calc button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            //объект рандомайзера
            Random rnd = new Random();

            //объект таймера
            Stopwatch sw = new Stopwatch();            

            //обозначаем размер матриц
            n = Convert.ToInt32(nMatrixSize.Text);

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
            matrix1Visual.Text = visualControl;

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
            matrix2Visual.Text = visualControl;

            //==========================================  ОБЫЧНЫЙ  ====================================

            //создаём 3ю матрицу регулярным методом
            C = new int[n, n];
            //пуск таймера
            sw.Start();
            for (int i = 0; i < n; i++)
            {
                for (int j =0; j<n; j++)
                {
                    for (int k = 0; k<n; k++)
                    {
                        C[i,j]=A[i,k]*B[k,j];
                    }
                }
            }
            //стоп таймера и вывод
            sw.Stop();
            regularTimeBox.Text= Convert.ToString(sw.ElapsedTicks); //ПОТОМ ПОПРАВИТЬ ТИКИ НА МИЛИСЕКУНДЫ

            //визуальный контроль сгенерированной регуляркой 3й матрицы
            visualControl = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) visualControl += Convert.ToString(C[i, j]) + "-";
            }
            matrix3Visual.Text = visualControl;

            //контрольная сумма регулярной матрицы
            int regularSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) regularSum += C[i, j];
            }
            regularSumBox.Text = Convert.ToString(regularSum);

            //=========================  МНОГОПОТОЧНЫЙ  =====================================================
            //создаём 3ю матрицу многопоточным методом
            //инициализируем матрицу
            Cmulti = new int[n, n];

            //получаем количество потоков
            int threadsCount = Convert.ToInt32(threadsBox.Text);

            //создаём массив потоков
            Thread[] threads = new Thread[threadsCount];

            //запуск потоков
            for (int i=0; i<threadsCount; i++)
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

            for (int i=0; i<threadsCount; i++)
            {
                //если итерация не последняя
                if (i != threadsCount - 1)
                {
                    start[i] = rowCounter;
                    end[i] = rowCounter + rowsPerThread +1 ;
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
            for (int i=0; i<threadsCount; i++)
            {              
                object startend = new int[] {start[i],end[i]};
                threads[i].Start(startend);
            }

            //ожидание завершения потоков
            for (int i=0; i<threadsCount; i++)
            {
                threads[i].Join();
            }

            //стоп таймера и вывод
            sw.Stop();
            multiTimeBox.Text = Convert.ToString(sw.ElapsedTicks); //ПОТОМ ПОПРАВИТЬ ТИКИ НА МИЛИСЕКУНДЫ

            //контрольная сумма многопоточной матрицы
            int multiSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) multiSum += Cmulti[i, j];
            }
            multiSumBox.Text = Convert.ToString(multiSum);


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
            parallelTimeBox.Text = Convert.ToString(sw.ElapsedTicks); //ПОТОМ ПОПРАВИТЬ ТИКИ НА МИЛИСЕКУНДЫ

            //контрольная сумма параллельной матрицы
            int parallelSum = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) parallelSum += Cparallel[i, j];
            }
            parallelSumBox.Text = Convert.ToString(parallelSum);

           
        }
    }
}
