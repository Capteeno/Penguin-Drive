using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Runtime.InteropServices; 
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        KinectSensor kinect;
        Skeleton[] skeletons;

        public Form1()
        {
            InitializeComponent();

            try
            {
                //kinectが接続されているかどうかを確認する
                if (KinectSensor.KinectSensors.Count == 0)
                {
                    throw new Exception("Kinetを接続してください");
                }
                //Kinectの動作を開始する
                kinect = KinectSensor.KinectSensors[0];
                StartKinect(kinect);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Kinectの動作を開始する
        /// </summary>
        /// <param name="Kinect"></param>

        private void StartKinect(KinectSensor kinect)
        {
            kinect.ColorStream.Enable();
            kinect.SkeletonStream.Enable();

            kinect.AllFramesReady +=
                new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);

            kinect.Start();
        }

        ///<summary>
        ///Kinectの動作を停止する
        ///</summary>
        ///<param name ="kinect"></param>
        private void StopKinect(KinectSensor kinect)
        {
            if (kinect != null)
            {
                if (kinect.IsRunning)
                {
                    kinect.AllFramesReady -= kinect_AllFramesReady;

                    kinect.Stop();
                    kinect.Dispose();

                    pictureBoxRgb.Image = null;
                }
            }
        }
        ///<summary>
        ///RGBカメラ、距離カメラ、骨格のフレーム更新イベント
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                //Kinectのインスタンスを取得する
                KinectSensor kinect = sender as KinectSensor;
                if (kinect == null)
                {
                    return;
                }

                //RGBカメラのフレームデータを取得する
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame != null)
                    {
                        //RGBカメラのピクセルデータを取得する
                        byte[] colorPixel = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(colorPixel);

                        //ピクセルデータをビットマップに変換する
                        Bitmap bitmap =
                            new Bitmap(kinect.ColorStream.FrameWidth,
                                       kinect.ColorStream.FrameHeight,
                                       System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                        BitmapData data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        Marshal.Copy(colorPixel, 0, data.Scan0, colorPixel.Length);
                        bitmap.UnlockBits(data);

                        pictureBoxRgb.Image = bitmap;
                    }
                }
                //スケルトンのフレームを取得する
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        const int R = 5;
                        Graphics g = Graphics.FromImage(pictureBoxRgb.Image);

                        skeletons =
                            new Skeleton[skeletonFrame.SkeletonArrayLength];
                        skeletonFrame.CopySkeletonDataTo(skeletons);

                        //トラッキングされているスケルトンのジョイントを描画する
                        foreach (var skeleton in skeletons)
                        {
                            //スケルトンがトラッキングされてなければ次へ
                            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                            {
                                continue;
                            }

                            //ジョイントを描画する
                            foreach (Joint joint in skeleton.Joints)
                            {
                                //ジョイントがトラッキングされていなければ次へ
                                if (joint.TrackingState != JointTrackingState.Tracked)
                                {
                                    continue;
                                }

                                //スケルトンの座標を、RGBカメラの座標に変換して円を書く
                                ColorImagePoint point = kinect.MapSkeletonPointToColor(joint.Position, kinect.ColorStream.Format);
                                g.DrawEllipse(new Pen(Brushes.Red),
                                  new Rectangle(point.X - R, point.Y - R, R * 2, R * 2));

                            }
                        }
                    }


                    //////////////////////////////////////////メソッド

                    Judge(kinect, skeletonFrame);

                //////////////////////////////////////////////
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Judge(KinectSensor kinect, SkeletonFrame skeletonFrame)
        {
            foreach (var skeleton in skeletons)
            {
                if (skeleton.Joints[JointType.ElbowLeft].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
                {
                    richTextBox1.Text += "左肘を下げてください";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;

            richTextBox1.Text = "";

            string text = richTextBox1.Text;


            foreach (var skeleton in skeletons)
            {
                //スケルトンがトラッキングされてなければ次へ
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }
                i++;
                richTextBox1.Text += "" + i + "人目の座標" + Environment.NewLine + "";

                //ジョイントを描画する
                foreach (Joint joint in skeleton.Joints)
                {
                    //ジョイントがトラッキングされていなければ次へ
                    if (joint.TrackingState != JointTrackingState.Tracked)
                    {
                        continue;
                    }

                    richTextBox1.Text += "-" + joint.Position.X + "" + Environment.NewLine + "";
                }


            }
        }

        private void pictureBoxRgb_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            {
                richTextBox1.Text = "";
                string text = richTextBox1.Text;
                richTextBox1.Text += "投球フォームを構えてください";

                foreach (var skeleton in skeletons)
                {
                    //スケルトンがトラッキングされてなければ次へ
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    {
                        continue;
                    }

                    //肩より手を上に（肩より手が下の時）
                    if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderRight].Position.Y || skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y)
                    {
                        richTextBox1.Text += "手を肩より上にあげてください";
                    }
                    else
                    {
                        continue;
                    }

                    //手を頭後方に持ってくる指示
                    if (skeleton.Joints[JointType.Head].Position.Z < skeleton.Joints[JointType.HandLeft].Position.Z || skeleton.Joints[JointType.Head].Position.Z < skeleton.Joints[JointType.HandLeft].Position.Z)
                    {
                        richTextBox1.Text += "手が頭の後ろになるようにしてください";
                        break;

                    }
                }
            }
            foreach (var skeleton in skeletons)
            {
                //スケルトンがトラッキングされてなければ次へ
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                int range1 = 5;
                    //左肘が右肘より高い時　左肘を下げる
                    if (skeleton.Joints[JointType.ElbowLeft].Position.Y - skeleton.Joints[JointType.ElbowRight].Position.Y > range1)
                    {
                        richTextBox1.Text += "左肘を下げてください";

                    }
                    //右肘が左肘より高い時　右肘を下げる
                    if (skeleton.Joints[JointType.ElbowLeft].Position.Y - skeleton.Joints[JointType.ElbowRight].Position.Y < -range1)
                    {
                        richTextBox1.Text += "右肘を下げてください";
                    }
                    //左肩が右肩より高い時　左肩を下げる
                    if (skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y > range1)
                    {
                        richTextBox1.Text += "左肩を下げてください";

                    }
                    //右肩が左肩より高い時　右肩を下げる
                    if (skeleton.Joints[JointType.ShoulderRight].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y < -range1)
                    {
                        richTextBox1.Text += "肩の高さを揃えてください";
                    }





                }

            }
        }
    }

    
   
    








    
