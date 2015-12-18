using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AirBand.Instruments;

namespace AirBand.Pages
{
    public partial class Page_Playing : UserControl, ISwitchable
    {
        private bool isSelectMode = false;
        private SoundHandler snd = null;
        private System.Windows.Forms.Timer debounceTimer;
        private System.Windows.Forms.Timer cancelDebounceTimer;
        private VM_Body body = Switcher.PageSwitcher.KinectHandler.TrackingBody;

        public Page_Playing()
        {
            InitializeComponent();
            DataContext = Switcher.VM_EnvironmentVariables;
        }

        public void EnterStory()
        {
            this.Begin("Enter", () => IsHitTestVisible = true);
        }

        public void ExitStory(Action callback)
        {
            IsHitTestVisible = false;
            this.Begin("Exit", () =>
            {
                Switcher.PageSwitcher.KinectHandler.InputEvent -= inputEvent;
                Switcher.PageSwitcher.KinectHandler.StopRead();
                callback();
            });
        }

        public void InitializeProperty()
        {
            Grid_Main.Opacity = 0;
            Btn_Cheer.RenderTransform = new TranslateTransform(140.8, 0);
            Btn_Gloom.RenderTransform = new TranslateTransform(140.8, 0);
            Btn_Inst.RenderTransform = new TranslateTransform(140.8, 0);
            Btn_Mask.RenderTransform = new TranslateTransform(140.8, 0);
            Btn_Exit.RenderTransform = new TranslateTransform(140.8, 0);
            Inst.Opacity = 0;
            Inst.Width = 50;
            Inst.Height = 50;
            Cnv_InstSelector.Opacity = 0;
            Cnv_InstSelector.RenderTransform = new RotateTransform(-90);
            Img_KinectFrame.Source = Switcher.PageSwitcher.KinectHandler.ImageSource;
            Img_UserView.Source = Switcher.PageSwitcher.KinectHandler.BodyIndexImageSource;
            debounceTimer = new System.Windows.Forms.Timer() { Interval = 1500 };
            cancelDebounceTimer = new System.Windows.Forms.Timer() { Interval = 1500 };
            snd = new SoundHandler();
            Switcher.PageSwitcher.KinectHandler.InputEvent += inputEvent;
            Switcher.PageSwitcher.MyoHandler.InputHandler += myoInputEvent;
        }

        private void inputEvent(KinectInputArgs e)
        {
            foreach (var element in Cnv_Main.Children)
                checkIntersects(element as Button);
            if (Cnv_InstSelector.IsHitTestVisible)
                foreach (var element in Cnv_InstSelector.Children)
                    checkIntersects(element as Button);
        }

        private void myoInputEvent()
        {
            Dispatcher.Invoke(() =>
            {
                if (Btn_Inst.IsHitTestVisible)
                {
                    Btn_Inst.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                else if (Btn_Return.IsHitTestVisible)
                {
                    Btn_Return.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            });
        }

        private void checkIntersects(Button btn)
        {
            if (btn != null)
            {
                if (btn.GetBounds(Cnv_Main).IntersectsWith(Switcher.PageSwitcher.Cur.GetBounds(Cnv_Main)))
                {
                    if (btn.IsHitTestVisible)
                    {
                        VisualStateManager.GoToState(btn, "MouseOver", false);
                        if (Switcher.PageSwitcher.KinectHandler.TrackingBody.RightHandState == Microsoft.Kinect.HandState.Lasso)
                            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
                else
                    VisualStateManager.GoToState(btn, "Normal", false);
            }
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeProperty();
            EnterStory();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Btn_Piano":
                    equipInst(0);
                    break;
                case "Btn_Guitar":
                    equipInst(1);
                    break;
                case "Btn_Drum":
                    equipInst(2);
                    break;
                case "Btn_Return":
                    exitInstSelector();
                    break;
                case "Btn_Cheer":
                    Btn_Cheer.IsHitTestVisible = Btn_Gloom.IsHitTestVisible = false;
                    Img_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Materials/CheerEffect.png", UriKind.Relative));
                    snd.PlayCheerSound();
                    this.Begin("Cheer", () => Btn_Cheer.IsHitTestVisible = Btn_Gloom.IsHitTestVisible = true);
                    break;
                case "Btn_Gloom":
                    Btn_Cheer.IsHitTestVisible = Btn_Gloom.IsHitTestVisible = false;
                    Img_Effect.Source = new BitmapImage(new Uri("/AirBand;component/Materials/GloomEffect.png", UriKind.Relative));
                    snd.PlayGloomSound();
                    this.Begin("Gloom", () => Btn_Cheer.IsHitTestVisible = Btn_Gloom.IsHitTestVisible = true);
                    break;
                case "Btn_Inst":
                    enterInstSelector();
                    break;
                case "Btn_Mask":
                    equipMask();
                    break;
                case "Btn_Exit":
                    Btn_Exit.IsHitTestVisible = false;
                    if (body.Instrument != null)
                        cancelInst();
                    else
                        Switcher.Switch(new Page_Main());
                    break;
                default:
                    break;
            }
        }

        private void cancelInst()
        {
            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
            body.ClearInstrument();
            cancelDebounceTimer.Enabled = true;
            cancelDebounceTimer.Tick += (s, ev) =>
            {
                cancelDebounceTimer.Enabled = false;
                Btn_Exit.IsHitTestVisible = true;
            };
        }

        private void equipInst(int id)
        {
            Grid_ForeControls.Children.Clear();
            if (body == null)
                return;
            switch (id)
            {
                case 0:
                    if (body.Instrument == null || body.Instrument.GetType() != typeof(Inst_Piano))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new Inst_Piano(Grid_ForeControls));
                    }
                    break;
                case 1:
                    if (body.Instrument == null || body.Instrument.GetType() != typeof(Inst_Guitar))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new Inst_Guitar(Grid_ForeControls));
                    }
                    break;
                case 2:
                    if (body.Instrument == null || body.Instrument.GetType() != typeof(Inst_Drum))
                    {
                        if (body.Instrument != null)
                            ((Grid)body.Instrument.Tag).Children.Remove(body.Instrument);
                        body.ClearInstrument();
                        body.SetInstrument(new Inst_Drum(Grid_ForeControls));
                    }
                    break;
                default:
                    break;
            }
            Grid_ForeControls.Children.Add(body.Instrument);
        }

        private void equipMask()
        {
            Btn_Mask.IsHitTestVisible = false;
            if (body != null && body.Mask == null)
            {
                body.SetMask(new Image() { Source = Switcher.VM_EnvironmentVariables.Mask });
                Cnv_Masks.Children.Add(body.Mask);
            }
            else if (body != null && body.Mask != null)
            {
                Cnv_Masks.Children.Remove(body.Mask);
                body.ClearMask();
            }
            debounceTimer.Enabled = true;
            debounceTimer.Tick += (s, ev) =>
            {
                debounceTimer.Enabled = false;
                Btn_Mask.IsHitTestVisible = true;
            };
        }

        private void enterInstSelector()
        {
            isSelectMode = Cnv_InstSelector.IsHitTestVisible = true;
            Btn_Inst.IsEnabled = Cnv_Main.IsHitTestVisible = !isSelectMode;
            this.Begin("EnterInstSelector");
        }

        private void exitInstSelector()
        {
            isSelectMode = Cnv_InstSelector.IsHitTestVisible = false;
            this.Begin("ExitInstSelector", () => Btn_Inst.IsEnabled = Cnv_Main.IsHitTestVisible = !isSelectMode);
        }
    }
}