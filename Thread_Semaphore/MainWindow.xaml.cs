using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Thread_Semaphore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public ObservableCollection<Thread> Threads { get; set; }

        public Semaphore semaphore { get; set; }

        public int Count { get; set; }

        public int ThreadCount { get; set; } = 1;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Count = 6;
            semaphore = new Semaphore(Count, Count, "Semaphoree");
            Threads = new();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() => SomeMethod());
            thread.Name = $"Thread {ThreadCount++}";
            Threads.Add(thread);
            createdbox.Items.Add(thread);
        }



        public void SomeMethod()
        {
            if (semaphore == null) return;
            bool isFinish = false;
            bool b = false;
            while (!isFinish)
            {
                if (semaphore.WaitOne(2000))
                {
                    Thread? Current = null;

                    Dispatcher.Invoke(() => Current = waitingbox.Items[0] as Thread);
                    if (Current == null) return;
                    try
                    {
                        Thread.Sleep(200);
                        Dispatcher.Invoke(() =>
                        {
                            Workingbox.Items.Add($"{Current.Name}->{Current.ManagedThreadId}");
                            waitingbox.Items.Remove(Current);
                        });
                        Thread.Sleep(6000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Workingbox.Items.Remove($"{Current.Name}->{Current.ManagedThreadId}");

                            semaphore.Release();
                            b = true;
                        });
                    }
                    finally
                    {
                        if (!b)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Workingbox.Items.Remove($"{Current.Name}->{Current.ManagedThreadId}");
                                semaphore.Release();
                            });
                        }
                        Threads.Remove(Current);
                        isFinish = true;
                    }
                }
            }
        }

        private void createdbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var l = sender as ListBox;
            var t = l?.SelectedItem as Thread;
            if (t == null) return;
            waitingbox.Items.Add(t);
            t?.Start();
            if (l != null) l.Items.Remove(t);
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int NewValue = (int)e.NewValue;
            int OldValue = Count;
            if (NewValue > OldValue)
            {
                semaphore.Release();
            }
            else if (NewValue < OldValue)
            {
                Thread thread = new Thread(() => semaphore.WaitOne());
                thread.Start();
            }
            Count = NewValue;
        }

        private void Workingbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedThread = Workingbox.SelectedItem as string;
            var threadName = selectedThread?.Split("->")[0].Trim();
            var thread = Threads.FirstOrDefault(t => t.Name == threadName);
            if (thread == null) return;
            if (thread.ThreadState == ThreadState.WaitSleepJoin) thread.Interrupt();
            Threads.Remove(thread!);
        }
    }

}
