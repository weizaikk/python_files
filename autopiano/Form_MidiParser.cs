using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;
using System.Collections;

namespace AutoFrame
{
    public partial class Form_MidiParser : Form
    {
        public Form_MidiParser()
        {
            InitializeComponent();
        }
        public DataGridView[] alldgv;
        public DataGridView[] dgv;
        public static DataGridView LeftDgv;
        public static DataGridView RightDgv;

        private void buttonLoadFile_Click(object sender, EventArgs e)
        {
            labelCurrFileName.Text = "MIDI文件:";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "midi文件|*.mid;*.MID|所有文件|*.*";
            dlg.FilterIndex = 1;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                labelCurrFileName.Text += dlg.SafeFileName;

                tabControlTracks.TabPages.Clear();
                dictTrackDataGridView.Clear();
                bool isLoadOK = midiFileParser.LoadFile(dlg.FileName);
                if (!isLoadOK)
                {
                    MessageBox.Show("", "打开MIDI文件失败", MessageBoxButtons.OK);
                    return;
                }

                int nTrackCount = midiFileParser.Count;//获取音轨数
                alldgv = new DataGridView[nTrackCount];
                for (int i = 0; i < nTrackCount; i++)
                {
                    TabPage tp = new TabPage("音轨" + i);
                    DataGridView trackDataGridView = new DataGridView();
                    trackDataGridView.AutoGenerateColumns = false;//禁止添加列
                    trackDataGridView.AllowUserToAddRows = false;
                    trackDataGridView.AllowUserToDeleteRows = false;
                    trackDataGridView.ScrollBars = ScrollBars.Vertical;//垂直滚动
                    //trackDataGridView.Name = "音轨" + i;
                    //trackDataGridView.Location = new System.Drawing.Point(30, 42);
                    //trackDataGridView.Size = new System.Drawing.Size(1200, 250);

                    trackDataGridView.ReadOnly = true;//只读
                    trackDataGridView.RowTemplate.Height = 23;//行高

                    trackDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                | System.Windows.Forms.AnchorStyles.Left)
                                | System.Windows.Forms.AnchorStyles.Right)));
                    trackDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                    //trackDataGridView.TabIndex = i;
                    AddTrackInfo2GridView(midiFileParser[i], trackDataGridView);
                    alldgv[i] = trackDataGridView;
                    tp.Controls.Add(trackDataGridView);
                    tabControlTracks.TabPages.Add(tp);
                    //tabControlTracks.Controls.Add(new DataGridView());//TabControl不能直接添加DataGridView控件

                }
            }
            selectTrack();
            Distribution();

            LeftDgv = DeleteControl(LeftDgv);
            RightDgv = DeleteControl(RightDgv);
        }

        /// <summary>
        /// 添加mid文件到dataGridView
        /// </summary>
        /// <param name="trakInfo"></param>
        /// <param name="gridView"></param>
        private void AddTrackInfo2GridView(Sanford.Multimedia.Midi.Track trakInfo, DataGridView gridView)
        {

            //DataGridViewTextBoxColumn boxColumn = new DataGridViewTextBoxColumn();

            DataGridViewTextBoxColumn ColumnEventIndex = new DataGridViewTextBoxColumn();
            ColumnEventIndex.HeaderText = "Event-Num";
            ColumnEventIndex.Name = "ColumnEventIndex";
            ColumnEventIndex.ReadOnly = true;
            //ColumnEventIndex.Width = 200;

            DataGridViewTextBoxColumn ColumnAbsTicks = new DataGridViewTextBoxColumn();
            ColumnAbsTicks.HeaderText = "Abs-Ticks";
            ColumnAbsTicks.Name = "ColumnAbsTicks";
            ColumnAbsTicks.ReadOnly = true;
            //ColumnAbsTicks.Width = 80;
            // 
            // ColumnDeltaTicks
            // 
            DataGridViewTextBoxColumn ColumnDeltaTicks = new DataGridViewTextBoxColumn();
            ColumnDeltaTicks.HeaderText = "Dlt Ticks";
            ColumnDeltaTicks.Name = "ColumnDeltaTicks";
            ColumnDeltaTicks.ReadOnly = true;
            // 
            // ColumnMsgStatus
            // 
            DataGridViewTextBoxColumn ColumnMsgStatus = new DataGridViewTextBoxColumn();
            ColumnMsgStatus.HeaderText = "MsgStatus";
            ColumnMsgStatus.Name = "ColumnMsgStatus";
            ColumnMsgStatus.ReadOnly = true;
            // 
            // ColumnMsgType
            // 
            DataGridViewTextBoxColumn ColumnMsgType = new DataGridViewTextBoxColumn();
            ColumnMsgType.HeaderText = "MsgType";
            ColumnMsgType.Name = "ColumnMsgType";
            ColumnMsgType.ReadOnly = true;
            // 
            // ColumnMsgSubType
            // 
            DataGridViewTextBoxColumn ColumnMsgSubType = new DataGridViewTextBoxColumn();
            ColumnMsgSubType.HeaderText = "SubType";
            ColumnMsgSubType.Name = "ColumnMsgSubType";
            ColumnMsgSubType.ReadOnly = true;
            // 
            // ColumnMsgData1
            // 
            DataGridViewTextBoxColumn ColumnMsgData1 = new DataGridViewTextBoxColumn();
            ColumnMsgData1.HeaderText = "MsgData1";
            ColumnMsgData1.Name = "ColumnMsgData1";
            ColumnMsgData1.ReadOnly = true;
            // 
            // ColumnMsgData2
            // 
            DataGridViewTextBoxColumn ColumnMsgData2 = new DataGridViewTextBoxColumn();
            ColumnMsgData2.HeaderText = "MsgData2";
            ColumnMsgData2.Name = "ColumnMsgData2";
            ColumnMsgData2.ReadOnly = true;
            // 
            // ColumnMidiChannel
            // 
            DataGridViewTextBoxColumn ColumnMidiChannel = new DataGridViewTextBoxColumn();
            ColumnMidiChannel.HeaderText = "MidiChannel";
            ColumnMidiChannel.Name = "ColumnMidiChannel";
            ColumnMidiChannel.ReadOnly = true;
            // 
            // ColumnBytes
            // 
            DataGridViewTextBoxColumn ColumnBytes = new DataGridViewTextBoxColumn();
            ColumnBytes.HeaderText = "Bytes";
            ColumnBytes.Name = "ColumnBytes";
            ColumnBytes.ReadOnly = true;

            gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            ColumnEventIndex,
            ColumnAbsTicks,
            ColumnDeltaTicks,
            ColumnMsgStatus,
            ColumnMsgType,
            ColumnMsgSubType,
            ColumnMsgData1,
            ColumnMsgData2,
            ColumnMidiChannel,
            ColumnBytes});
            for (int i = 0; i < trakInfo.Count; i++)
            {
                MidiEvent me = trakInfo.GetMidiEvent(i);
                //listViewTrackInfo.Items.Add(string.Format("音轨{0},EventNo = {1} Msg type = {2} , status = {3}", i, j, me.MidiMessage.MessageType, me.MidiMessage.Status));
                ArrayList stuff = new ArrayList();
                stuff.Add(i);
                stuff.Add(me.AbsoluteTicks);
                stuff.Add(me.DeltaTicks);
                stuff.Add(me.MidiMessage.Status);
                stuff.Add(me.MidiMessage.MessageType);
                switch (me.MidiMessage.MessageType)
                {
                    case MessageType.Channel:
                        ChannelMessage cm = (ChannelMessage)me.MidiMessage;
                        stuff.Add(cm.Command);
                        stuff.Add(cm.Data1);
                        stuff.Add(cm.Data2);
                        stuff.Add(cm.MidiChannel);
                        break;

                    case MessageType.SystemExclusive:
                        SysExMessage sem = (SysExMessage)me.MidiMessage;
                        stuff.Add(sem.SysExType);

                        break;

                    case MessageType.SystemCommon:
                        SysCommonMessage scm = (SysCommonMessage)me.MidiMessage;
                        stuff.Add(scm.SysCommonType);
                        stuff.Add(scm.Data1);
                        stuff.Add(scm.Data2);

                        break;

                    case MessageType.SystemRealtime:
                        SysRealtimeMessage srm = (SysRealtimeMessage)me.MidiMessage;
                        stuff.Add(srm.SysRealtimeType);

                        break;
                    case MessageType.Meta:
                        MetaMessage mm = (MetaMessage)me.MidiMessage;
                        stuff.Add(mm.MetaType);
                        //if (mm.MetaType == MetaType.TrackName)
                        //{
                        //    stuff.Add(""); //data1
                        //    stuff.Add(""); //data2
                        //    stuff.Add("");//MidiChannel
                        //    byte[] bytesTrkName = mm.GetBytes();
                        //    //UnicodeEncoding encoding = new UnicodeEncoding();
                        //    //string trackName = encoding.GetString(mm.GetBytes());
                        //    //stuff.Add(/*trackName*/mm.GetBytes());
                        //}
                        break;
                }

                //object[] stuff = { i, j, me.MidiMessage.MessageType,me.AbsoluteTicks };
                gridView.Rows.Add(stuff.ToArray());
            }
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e) //Tab选项改变
        {

        }

        private MidiFileParser midiFileParser = new MidiFileParser();//Midi文件解析器对象
        Dictionary<int, DataGridView> dictTrackDataGridView = new Dictionary<int, DataGridView>();//

        /// <summary>
        /// 筛选出两个最长的音轨
        /// </summary>
        /// <returns></returns>
        public DataGridView[] selectTrack()
        {

            if (midiFileParser.Count == 1)
            {
                dgv = new DataGridView[1];
                return alldgv;
            }
            else
            {
                int max1 = 0;
                int max2 = 0;
                int max = 0;
                dgv = new DataGridView[2];
                foreach (DataGridView d in alldgv)
                {

                    int note = 0, control = 0;
                    foreach (DataGridViewRow drc in d.Rows)
                    {
                        string str = drc.Cells[5].Value.ToString();

                        if (str == "NoteOn" || str == "NoteOff")
                        {
                            note++;
                        }
                        else
                        {
                            control++;
                        }
                    }
                    //选取出noteon或者noteoff类型最多的两组音轨来进行演奏
                    max = note;
                    if (note == 0)
                    {
                        if (max1 ==0)
                        {
                            dgv[0] = null;
                            dgv[1] = null;
                        }
                        if (max2 == 0)
                        {
                            dgv[1] = null;
                        }

                        continue;
                    }
                    else
                    {
                        if (max1 == 0)
                        {
                            max1 = note;
                            dgv[0] = d;
                        }
                        else if (max2 == 0)
                        {
                            max2 = note;
                            dgv[1] = d;
                        }
                        else if (note >= max1 && note >= max2)//1,3;3,1
                        {
                            max1 = note;
                            dgv[0] = d;
                        }
                        else if (note >= max1 && note <= max2)
                        {
                            max1 = note;
                            dgv[0] = d;
                        }
                        else if (note <= max1 && note >= max2)
                        {
                            max2 = note;
                            dgv[1] = d;
                        }
                        else
                        {

                        }
                    }

                }
            }
            return dgv;
        }

        /// <summary>
        /// 将两个音轨合理分配给左右手
        /// </summary>
        public void Distribution()
        {
            long k0 = 0;
            long k1 = 0;
            if (dgv[0] != null)
            {
                foreach (DataGridViewRow dr in dgv[0].Rows)
                {
                    if (dr.Cells[6].Value != null)
                    {
                        k0 = k0 + int.Parse(dr.Cells[6].Value.ToString());
                    }
                }
                if (dgv[1] != null)
                {
                    foreach (DataGridViewRow dr in dgv[1].Rows)
                    {
                        if (dr.Cells[6].Value != null)
                        {
                            k1 = k1 + int.Parse(dr.Cells[6].Value.ToString());
                        }
                    }
                    if (k1 / dgv[1].RowCount > k0 / dgv[0].RowCount)
                    {
                        LeftDgv = dgv[0];
                        RightDgv = dgv[1];
                    }
                    else
                    {
                        LeftDgv = dgv[1];
                        RightDgv = dgv[0];
                    }
                }
                else
                {
                    if (k0 >= 60)
                    {
                        LeftDgv = null;
                        RightDgv = dgv[0];
                    }
                    else
                    {
                        LeftDgv = dgv[0];
                        RightDgv = null;
                    }
                }
            }
            else
            {
                MessageBox.Show("尚未选择歌曲或文件不对！\n先点击确定，停止运行软件，然后在文件按钮下点击 加载MIDI文件");
            }
        }

        /// <summary>
        /// 删除非note的行
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        public DataGridView DeleteControl(DataGridView dd)
        {
            try
            {
                DataGridView dnew = new DataGridView();
                dnew.Columns.AddRange(new DataGridViewColumn[9]
                {
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                     new DataGridViewTextBoxColumn(),
                });
                if (dd == null)
                {
                    return null;
                }
                else
                {
                    foreach (DataGridViewRow dr in dd.Rows)
                    {
                        ArrayList stuff = new ArrayList();
                        string str = dr.Cells[5].Value.ToString();
                        if (str == "NoteOn" || str == "NoteOff")
                        {
                            foreach (DataGridViewCell s in dr.Cells)
                            {

                                if (s.Value != null)
                                {
                                    string s1 = s.Value.ToString();
                                    stuff.Add(s1);
                                }
                                else
                                {
                                    stuff.Add(0);
                                }
                            }
                            dnew.Rows.Add(stuff.ToArray());
                        }
                    }
                    return dnew;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
