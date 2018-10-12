using System;
using System.Threading;
using AutoFrameDll;
using CommonTool;
using Communicate;
using AutoFrameVision;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoFrame
{
    /// <summary>
    /// 钢琴演奏右机器人
    /// </summary>
    class StationRightRobot : StationBase
    {
        private TcpLink m_tcpRobotCtl;
        public TimeSpan ticks = TimeSpan.FromMilliseconds(10);
        public StationRightRobot(string strName) : base(strName)
        {
            io_in = new string[] { };
            io_out = new string[] { };
            bPositiveMove = new bool[] { true, true, true, true };
            strAxisName = new string[] { "X轴", "Y轴", "Z轴", "U轴" };

            m_tcpRobotCtl = TcpMgr.GetInstance().GetTcpLink(1);
        }

        public override void InitSecurityState()
        {
            SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.RightRobotReady, false);
            SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.RightHandlePlayOver, false);
        }

        //public override bool IsReady()
        //{
        //    return true;
        //}

        public override void EmgStop()
        {
            //if (m_tcpRobotCtl.IsOpen())
            //    m_tcpRobotCtl.WriteLine("Stop");//机器人停止命令
        }//站位急停

        public override void StationDeinit()
        {
            ShowLog("关闭机器人控制和通讯网口");
            if (m_tcpRobotCtl.IsOpen())
                m_tcpRobotCtl.WriteLine("Stop");//机器人停止命令
            WaitTimeDelay(100);
            m_tcpRobotCtl.Close();
        }

        public override void StationInit()
        {
            ShowLog("打开右机器人网口");
            m_tcpRobotCtl.Open();
            Thread.Sleep(200);
            if (m_tcpRobotCtl.IsOpen())
            {
                m_tcpRobotCtl.WriteLine("Home");
                wait_recevie_cmd(m_tcpRobotCtl, "BackHomeOK", 20000, true, false);
            }
            else
            {
                throw new Exception("右机器人网口打开失败！");
            }

            SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.RightRobotReady, true);
        }

        public DataGridView Rightdgv = new DataGridView();
        public override void StationProcess()
        {
            WaitTimeDelay(10);
            Rightdgv = Form_MidiParser.RightDgv;

            if (true == SystemMgr.GetInstance().GetRegBit((int)SysBitReg.LeftRobotReady) &&
                true == SystemMgr.GetInstance().GetRegBit((int)SysBitReg.RightRobotReady))
            {
                RobotMove();
                WaitTimeDelay(2000);
            }
        }

        public static int currentPos_R = 0;
        /// <summary>
        /// 机器人动作
        /// </summary>
        public void RobotMove()
        {
            if (Rightdgv != null)
            {
                try
                {
                    int count = Rightdgv.Rows.Count - 2;
                    string ten_str = "";
                    int ten_count = 0;

                    m_tcpRobotCtl.WriteLine("Move");//开始执行动作，返回Goon
                    WaitTimeDelay(int.Parse(Rightdgv.Rows[0].Cells[2].Value.ToString()) * Pos.minTickTime);  //等第一个时间差
                    wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);

                    for (int i = 0; i < count; i++)
                    {
                        
                        CheckContinue();    
                        int data1 = int.Parse(Rightdgv.Rows[i].Cells[6].Value.ToString());    //循环获取每一行的mid键位
                        int data2 = int.Parse(Rightdgv.Rows[i].Cells[7].Value.ToString());    //获取乐器类型
                        int time = int.Parse(Rightdgv.Rows[i + 1].Cells[2].Value.ToString()); //获取下一行的时间差(Dlt Ticks)
                        int key = KeyReplace(ChangeBlackKeyToWhite(data1));//转换成机器人的点位
                        string note = Rightdgv.Rows[i].Cells[5].Value.ToString();  //获取note值

                        if (key == 0)
                        {
                            continue;
                            //throw new Exception("按键为0，不在范围之内。错误");
                        }

                        if (note == "NoteOn" || note == "noteon")
                        {
                            currentPos_R = key;
                            if (ConfirmSafe(StationLeftRobot.currentPos_L, StationRightRobot.currentPos_R))
                            {
                                //一次发送10组数
                                //wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                                string str = string.Format("{0},{1},", key, DelayTimes(time));
                                ten_str = ten_str + str;
                                ten_count++;
                                if (ten_count%10==0)
                                {
                                    wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                                    m_tcpRobotCtl.WriteLine(ten_str + "\r\n");
                                    //wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                                    ten_str = "";
                                }
                                else if (i == count - 1)
                                {
                                    int m = ten_count % 10;
                                    for (int k =0;k<10-m;k++)
                                    {
                                        ten_str += "0,0,";
                                    }
                                    wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                                    m_tcpRobotCtl.WriteLine(ten_str + "\r\n");
                                    
                                    //wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                                }
                                
                                

                                

                                //一次发送一点
                                //wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);

                                //string strs = string.Format("{0},{1},{2}", key, DelayTimes(time), "\r\n");
                                //m_tcpRobotCtl.WriteLine(strs);
                            }
                            else
                            {
                                throw new Exception("位置错误，运动会碰撞！");
                            }
                        }
                        else if (note == "NoteOff" || note == "noteoff")
                        {
                            //正常情况不弹
                            WaitTimeDelay(time * Pos.minTickTime);
                        }
                    }
                    //歌曲演奏完毕，回原，等待另一只手结束
                    wait_recevie_cmd(m_tcpRobotCtl, "Goon", 20000, true, false);
                    m_tcpRobotCtl.WriteLine("Home");
                    currentPos_R = 0;

                    SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.RightHandlePlayOver, true);
                    while (true)
                    {
                        if (true == SystemMgr.GetInstance().GetRegBit((int)SysBitReg.LeftHandlePlayOver))
                        {
                            SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.LeftHandlePlayOver, false);
                            break;
                        }
                        WaitTimeDelay(SystemMgr.GetInstance().ScanTime);
                        CheckContinue();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                //单音轨时，回原，不操作
                m_tcpRobotCtl.WriteLine("Home");
                currentPos_R = 0;
                SystemMgr.GetInstance().WriteRegBit((int)SysBitReg.RightHandlePlayOver, true);

                WaitTimeDelay(2000);
                CheckContinue();
            }
        }

        /// <summary>
        /// 右手1-14号键替换为15-28号键，并转换成机器人的点位
        /// </summary>
        /// <param name="n_Key"></param>
        /// <returns></returns>
        public int KeyReplace(int n_Key)
        {
            if (n_Key >= 15 && n_Key <= 36) return n_Key - 14;
            else if (n_Key == 1) return 1;
            else if (n_Key == 2) return 2;
            else if (n_Key == 3) return 3;
            else if (n_Key == 4) return 4;
            else if (n_Key == 5) return 5;
            else if (n_Key == 6) return 6;
            else if (n_Key == 7) return 7;

            else if (n_Key == 8) return 8;
            else if (n_Key == 9) return 9;
            else if (n_Key == 10) return 10;
            else if (n_Key == 11) return 11;
            else if (n_Key == 12) return 12;
            else if (n_Key == 13) return 13;
            else if (n_Key == 14) return 14;
            else return 0;
        }

        /// <summary>
        /// 根据两个手当前的键位位置差来防撞，true为安全
        /// </summary>
        /// <param name="n_KeyL">左手当前位置(机器人的基准)</param>
        /// <param name="n_KeyR">右手当前位置(机器人的基准)</param>
		/// 和currentPos_R配合使用
        /// <returns></returns>
        public bool ConfirmSafe(int n_KeyL, int n_KeyR)
        {
            if (n_KeyL == 0)//初始化时，说明左边在原点，另一边可以先动作(随便动)
            {
                return true;
            }
            else
            {
                if (RobotToKey(n_KeyR, "Right") - RobotToKey(n_KeyL, "Left") >= 2) return true;
                else return false;
            }
        }

        /// <summary>
        /// 按键加延时
        /// </summary>
        /// <param name="n_time"></param>
        /// <returns></returns>
        public int DelayTimes(int n_time)
        {
            if (n_time == 0) return 0;
            //else if (n_time < 50) return n_time * Pos.minTickTime + 200;
            //else return n_time * Pos.minTickTime;
            else return n_time * Pos.minTickTime;
        }

        /// <summary>
        /// 将机器人的点位转换成对应的键位
        /// </summary>
        /// <param name="m_Key">机器人的点位序号</param>
        /// <param name="nTyple">左/右类型</param>
        /// <returns></returns>
        public int RobotToKey(int m_Key, string nTyple)
        {
            if (nTyple == "Left" || nTyple == "left")
            {
                if (m_Key >= 1 && m_Key <= 22) return m_Key;
                else return 0;
            }
            else if (nTyple == "Right" || nTyple == "right")
            {
                if (m_Key >= 1 && m_Key <= 22) return m_Key + 14;
                else return 0;
            }
            else return 0;
        }

        /// <summary>
        /// 将序列中的oldkey转换成现实中的key（从1开始，同时将黑键用相邻白建代替,一共36个白建）
        /// </summary>
        /// <param name="OldKey"></param>
        /// <returns></returns>
        public int ChangeBlackKeyToWhite(int OldKey)
        {
            if (OldKey == 38) return 1;
            else if (OldKey == 39) return 1;
            else if (OldKey == 40) return 2;
            else if (OldKey == 41) return 2;
            else if (OldKey == 42) return 3;
            else if (OldKey == 43) return 4;
            else if (OldKey == 44) return 4;
            else if (OldKey == 45) return 5;
            else if (OldKey == 46) return 5;
            else if (OldKey == 47) return 6;
            else if (OldKey == 48) return 6;
            else if (OldKey == 49) return 7;

            else if (OldKey == 50) return 8;
            else if (OldKey == 51) return 8;
            else if (OldKey == 52) return 9;
            else if (OldKey == 53) return 9;
            else if (OldKey == 54) return 10;
            else if (OldKey == 55) return 11;
            else if (OldKey == 56) return 11;
            else if (OldKey == 57) return 12;
            else if (OldKey == 58) return 12;
            else if (OldKey == 59) return 13;
            else if (OldKey == 60) return 14;
            else if (OldKey == 61) return 14;//右手极限点

            else if (OldKey == 62) return 15;
            else if (OldKey == 63) return 15;
            else if (OldKey == 64) return 16;
            else if (OldKey == 65) return 16;
            else if (OldKey == 66) return 17;
            else if (OldKey == 67) return 18;
            else if (OldKey == 68) return 18;
            else if (OldKey == 69) return 19;
            else if (OldKey == 70) return 19;
            else if (OldKey == 71) return 20;
            else if (OldKey == 72) return 21;
            else if (OldKey == 73) return 21;

            else if (OldKey == 74) return 22;
            else if (OldKey == 75) return 22;//左手极限点
            else if (OldKey == 76) return 23;
            else if (OldKey == 77) return 24;
            else if (OldKey == 78) return 24;
            else if (OldKey == 79) return 25;
            else if (OldKey == 80) return 25;
            else if (OldKey == 81) return 26;
            else if (OldKey == 82) return 26;
            else if (OldKey == 83) return 27;
            else if (OldKey == 84) return 28;
            else if (OldKey == 85) return 28;

            else if (OldKey == 86) return 29;
            else if (OldKey == 87) return 29;
            else if (OldKey == 88) return 30;
            else if (OldKey == 89) return 30;
            else if (OldKey == 90) return 31;
            else if (OldKey == 91) return 32;
            else if (OldKey == 92) return 32;
            else if (OldKey == 93) return 33;
            else if (OldKey == 94) return 33;
            else if (OldKey == 95) return 34;
            else if (OldKey == 96) return 35;
            else if (OldKey == 97) return 35;

            else if (OldKey == 98) return 36;
            else return 0;//其它的键全部抛弃
        }

        /// <summary>
        /// 将实际的键(36个白健)转换成对应的音阶，7个一组
        /// </summary>
        /// <param name="OldKey"></param>
        /// <returns></returns>
        public int ChangeKeyTodori(int OldKey)
        {

            if (OldKey == 1) return 1;
            else if (OldKey == 2) return 2;
            else if (OldKey == 3) return 3;
            else if (OldKey == 4) return 4;
            else if (OldKey == 5) return 5;
            else if (OldKey == 6) return 6;
            else if (OldKey == 7) return 7;

            else if (OldKey == 8) return 1;//36
            else if (OldKey == 9) return 2;//23---左手23-36号替换到8-9-21
            else if (OldKey == 10) return 3;//24
            else if (OldKey == 11) return 4;//25
            else if (OldKey == 12) return 5;//26
            else if (OldKey == 13) return 6;//27
            else if (OldKey == 14) return 7;//28

            else if (OldKey == 15) return 1;//29
            else if (OldKey == 16) return 2;//30
            else if (OldKey == 17) return 3;//31
            else if (OldKey == 18) return 4;//32
            else if (OldKey == 19) return 5;//33
            else if (OldKey == 20) return 6;//34
            else if (OldKey == 21) return 7;//35

            else if (OldKey == 22) return 1;//1---右手1-13号替换到22-34
            else if (OldKey == 23) return 2;//2
            else if (OldKey == 24) return 3;//3
            else if (OldKey == 25) return 4;//4
            else if (OldKey == 26) return 5;//5
            else if (OldKey == 27) return 6;//6
            else if (OldKey == 28) return 7;//7

            else if (OldKey == 29) return 1;//8
            else if (OldKey == 30) return 2;//9
            else if (OldKey == 31) return 3;//10
            else if (OldKey == 32) return 4;//11
            else if (OldKey == 33) return 5;//12
            else if (OldKey == 34) return 6;//13
            else if (OldKey == 35) return 7;

            else if (OldKey == 36) return 1;
            else return 0;
        }
    }
}

