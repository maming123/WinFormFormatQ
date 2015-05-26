using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace WinFormFormatQ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 知识点
        private void button1_Click(object sender, EventArgs e)
        {
            string str = this.txtBoxOrign.Text.Trim();

            string[] arr = GetArrZhiShiDian(str);

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?><root>");
            sb.AppendFormat(@"<sectionTitle name=""{0}"">", arr[0]);

            for (int i = 1; i < arr.Length; i++)
            {
                if (Regex.IsMatch(arr[i], @"第[\d]+章", RegexOptions.Singleline))
                {
                    if (Regex.IsMatch(sb.ToString(), @"第[\d]+章", RegexOptions.Singleline))
                    {
                        sb.AppendFormat(@"""/></sectionTitle>");
                    }
                    sb.AppendFormat(@"<sectionTitle name=""{0}"">", arr[i]);
                }
                else if (arr[i].Contains("[") && arr[i].Contains("]"))
                {
                    if (sb.ToString().Contains("[") && sb.ToString().Contains("[")
                        && !(Regex.IsMatch(arr[i-1], @"第[\d]+章", RegexOptions.Singleline)))
                    {
                        sb.AppendFormat(@"""/>");
                    }
                    sb.AppendFormat(@"<point question = ""{0}"" answer = """, arr[i]);
                }
                else
                {
                    sb.AppendFormat(@" {0} ", arr[i]);
                }
            }

            sb.AppendFormat(@"""/></sectionTitle>");
            sb.Append("</root>");
            this.txtBoxC1.Text = sb.ToString();
        }

        private static string[] GetArrZhiShiDian(string str)
        {
            string[] arr = str.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //把 内容中的\n替换掉 多行换成1行
            //整理后问道类似
            /***
             * 第1章
             * [1] xxxxx
             * 内容xxxx
             * [2] xxxx
             * 内容xxxx
             * ....
             * [n] xxxx
             * 第n章
             * ...
             * **/
            List<string> listOrig = new List<string>();
            listOrig.AddRange(arr);
            List<string> listNew = new List<string>();
            string strtmp = "";
            listOrig.ForEach(m =>
            {

                if (m.Contains("第") && m.Contains("章"))
                {
                    if (strtmp != "")
                    {
                        listNew.Add("\\n" + strtmp);
                        strtmp = "";
                    }
                    listNew.Add(m);
                    //listOrig.Remove(m);

                }
                else if (m.Contains("[") && m.Contains("]"))
                {
                    if (strtmp != "")
                    {
                        listNew.Add("\\n" + strtmp);
                        strtmp = "";
                    }

                    listNew.Add(m);
                    //listOrig.Remove(m);

                }
                else
                {
                    strtmp += "\\n" + m + " ";
                    //listOrig.Remove(m);
                }

            });

            if (strtmp != "")
            {
                listNew.Add(strtmp);
                strtmp = "";
            }
            List<string> listResult = new List<string>();
            listNew.ForEach(m =>
                {
                    string strtmp2 = m.Trim();

                    if (strtmp2.IndexOf("\\n\\n") == 0)
                    {
                        listResult.Add(strtmp2.Substring(4));
                    }
                    else if (strtmp2.IndexOf("\\n") == 0)
                    {
                        listResult.Add(strtmp2.Substring(2));
                    }
                    else
                    {
                        listResult.Add(strtmp2);
                    }
                });
            return listResult.ToArray();
        }
        #endregion

        #region 选择题
        /// <summary>
        /// 打开选择题文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox1.Text = this.openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listOrig = new List<string>();

            string strQ = "";
            using (StreamReader sr = new StreamReader(this.textBox1.Text.Trim()))
            {
                while (sr.Peek() > -1)
                {
                    string str = sr.ReadLine().Trim();
                    if (!this.checkBox1.Checked)
                    {
                        if (!string.IsNullOrEmpty(str) && "ABCDEFGHIＡＢＣＤＥＦＧＨＩ".Contains(str.Substring(0, 1)))
                        {
                            str = str.Insert(1, ".");
                        }
                        if (!string.IsNullOrEmpty(str) && Regex.IsMatch(str, @"^[\d]+", RegexOptions.Singleline))
                        {
                            string s = Regex.Match(str, @"^[\d]+").Value;
                            str = str.Insert(s.Length, ".");
                        }
                    }
                    //[第\d章]
                    if (Regex.IsMatch(str, @"第[\d]+章", RegexOptions.Singleline))
                    {
                        if (!string.IsNullOrEmpty(strQ))
                        {
                            listOrig.Add(strQ);
                        }
                        listOrig.Add("$ZJ$" + str);
                        strQ = "";
                        
                    }

                    if (Regex.IsMatch(str, @"^[\d]+[．.]", RegexOptions.Singleline))
                    {
                        listOrig.Add(strQ);
                        strQ = " $TM$ " + str;
                    }
                    else if (Regex.IsMatch(str, @"[ABCDEFGHIＡＢＣＤＥＦＧＨＩ][ 　．.]+", RegexOptions.Singleline))
                    {
                        //经过以下替换 把 字母 A.A．Ａ．Ａ. 替换成$TAB$A．
                        str = str.Replace("A．", "$TAB$A．").Replace("B．", "$TAB$B．").Replace("C．", "$TAB$C．").Replace("D．", "$TAB$D．").Replace("E．", "$TAB$E．").Replace("F．", "$TAB$F．").Replace("G．", "$TAB$G．").Replace("H．", "$TAB$H．").Replace("I．", "$TAB$I．").Replace("Ａ．", "$TAB$A．").Replace("Ｂ．", "$TAB$B．").Replace("Ｃ．", "$TAB$C．").Replace("Ｄ．", "$TAB$D．").Replace("Ｅ．", "$TAB$E．").Replace("Ｆ．", "$TAB$F．").Replace("Ｇ．", "$TAB$G．").Replace("Ｈ．", "$TAB$H．").Replace("Ｉ．", "$TAB$I．").Replace("A ．", "$TAB$A．").Replace("B ．", "$TAB$B．").Replace("C ．", "$TAB$C．").Replace("D ．", "$TAB$D．").Replace("E ．", "$TAB$E．").Replace("F ．", "$TAB$F．").Replace("G ．", "$TAB$G．").Replace("H ．", "$TAB$H．").Replace("I ．", "$TAB$I．");

                        str = str.Replace("A.", "$TAB$A．").Replace("B.", "$TAB$B．").Replace("C.", "$TAB$C．").Replace("D.", "$TAB$D．").Replace("E.", "$TAB$E．").Replace("F.", "$TAB$F．").Replace("G.", "$TAB$G．").Replace("H.", "$TAB$H．").Replace("I.", "$TAB$I．").Replace("Ａ.", "$TAB$A．").Replace("Ｂ.", "$TAB$B．").Replace("Ｃ.", "$TAB$C．").Replace("Ｄ.", "$TAB$D．").Replace("Ｅ.", "$TAB$E．").Replace("Ｆ.", "$TAB$F．").Replace("Ｇ.", "$TAB$G．").Replace("Ｈ.", "$TAB$H．").Replace("Ｉ.", "$TAB$I．");

                        strQ += str;
                    }
                }
            }
            listOrig.Add(strQ);
            listOrig.ForEach(m =>
            {
                this.txtBoxSelectResult.Text += m + "\n";
            }
            );

        }

        /// <summary>
        /// 选择题处理步骤2  处理第一步加工过的选择题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string str = this.txtBoxSelectResult.Text.Trim().Replace("\n", "");
            string[] arr = str.Split(new string[] { "$ZJ$" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sbAll = new StringBuilder();
            sbAll.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?><root>");
            for (int i = 0; i < arr.Length; i++)
            {
                //章节
                string[] arrZhang = arr[i].Split(new string[] { "$TM$" }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"<sectionTitle name=""{0}"">", arrZhang[0]);
                sb.AppendFormat(@"<testType type=""c"" name=""选择题"">");

                for (int timuNum = 1; timuNum < arrZhang.Length; timuNum++)
                { //每章
                    //<question name = "3．免疫对机体是（ ）" opa = "A．有害的" opb = "B．有利的" opc = "C．有利也有害" opd = "D．有利无害" ope = "E．正常条件下有利，异常条件下有害" />
                    string strtmp = arrZhang[timuNum].Replace("$TAB$A．", "\" opa = \"A．").Replace("$TAB$B．", "\" opb = \"B．").Replace("$TAB$C．", "\" opc = \"C．").Replace("$TAB$D．", "\" opd = \"D．").Replace("$TAB$E．", "\" ope = \"E．").Replace("$TAB$F．", "\" opf = \"F．").Replace("$TAB$G．", "\" opg = \"G．").Replace("$TAB$H．", "\" oph = \"H．").Replace("$TAB$I．", "\" opi = \"I．");
                    sb.AppendFormat(@"<question  type=""A1"" name = ""{0}""/>", strtmp);
                }
                sb.AppendFormat(@"</testType>");
                sb.AppendFormat(@"</sectionTitle>");
                sbAll.Append(sb.ToString());
            }
            sbAll.Append("</root>");
            this.txtBoxSelectResult.Text = sbAll.ToString();
        }
        #endregion

        #region 问答题
        /// <summary>
        /// 问答题 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox2.Text = this.openFileDialog1.FileName;
            }

        }

        /// <summary>
        /// 问答题处理步骤1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listOrig = new List<string>();

            using (StreamReader sr = new StreamReader(this.textBox2.Text.Trim()))
            {
                while (sr.Peek() > -1)
                {
                    string str = sr.ReadLine().Trim();
                    str = str.Replace("       ", "________");
                    //[第\d章]
                    if (Regex.IsMatch(str, @"第[\d]+章", RegexOptions.Singleline) && !str.Contains("答案"))
                    {
                        str = "$ZJ$" + str;
                    }
                    else
                    {
                        str = "$TM$" + str;
                    }
                    listOrig.Add(str);
                }
            }
            listOrig.ForEach(m =>
            {
                this.txtBoxQResult.Text += m + "\n";
            }
            );
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string str = this.txtBoxQResult.Text.Trim().Replace("\n", "");
            string[] arr = str.Split(new string[] { "$ZJ$" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sbAll = new StringBuilder();
            sbAll.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?><root>");
            for (int i = 0; i < arr.Length; i++)
            {
                //章节
                string[] arrZhang = arr[i].Split(new string[] { "$TM$" }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"<sectionTitle name=""{0}"">", arrZhang[0]);

                string strtmp = "";
                for (int timuNum = 1; timuNum < arrZhang.Length; timuNum++)
                {   //每章
                    //<testType type="q" name="练习题">
                    //    <question name = "1．_________________________________________，称为现代免疫的概念。" />
                    //    <question name="2._______、_______和_______，称为免疫的三大功能。"/>
                    //</testType>
                    //<testType type="a" name="第1章_参考答案">
                    //    <answer name="1．机体识别和排除抗原物质，维持自身平衡与稳定的一种生理功能"/>
                    //    <answer name="2．免疫防御 免疫稳定 免疫监视"/>
                    //</testType>

                    if (arrZhang[timuNum].Contains("练习题") && !arrZhang[timuNum].Contains("答案"))
                    {
                        sb.AppendFormat(@"<testType type=""q"" name=""{0}"">", arrZhang[timuNum]);
                        strtmp = "question";
                    }
                    else if (arrZhang[timuNum].Contains("答案") && arrZhang[timuNum].IndexOf("答案") == 0)
                    {
                        sb.AppendFormat(@"</testType>");
                        sb.AppendFormat(@"<testType type=""a"" name=""{0}"">", arrZhang[timuNum]);
                        strtmp = "answer";
                    }
                    else
                    {
                        sb.AppendFormat(@"<{0} name=""{1}""/>", strtmp, arrZhang[timuNum]);
                    }

                }
                sb.AppendFormat(@"</testType>");
                sb.AppendFormat(@"</sectionTitle>");
                sbAll.Append(sb.ToString());
            }
            sbAll.Append("</root>");
            this.txtBoxQResult.Text = sbAll.ToString();

        }
        #endregion

        #region 试卷一 去除题干
        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                this.txtBoxTest1.Text = this.openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// 试卷处理一 第一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listOrig = new List<string>();

            string strQ = "";
            using (StreamReader sr = new StreamReader(this.txtBoxTest1.Text.Trim()))
            {
                while (sr.Peek() > -1)
                {
                    string str = sr.ReadLine().Trim();
                    //if (str.Contains("103.4kPa"))
                    {
                    }
                    //[第\d章]
                    if (Regex.IsMatch(str, @"^专业实务", RegexOptions.Singleline)
                        || Regex.IsMatch(str, @"^实践能力", RegexOptions.Singleline))
                    {
                        listOrig.Add("$ZJ$" + str);
                        strQ = "";
                    }
                    else if (Regex.IsMatch(str, @"^[\d]+[．.]", RegexOptions.Singleline))
                    {
                        listOrig.Add(strQ);
                        strQ = " $TM$ " + str;
                    }
                    else if (Regex.IsMatch(str, @"[ABCDEFGHIＡＢＣＤＥＦＧＨＩ][ 　．.]+", RegexOptions.Singleline))
                    {
                        //经过以下替换 把 字母 A.A．Ａ．Ａ. 替换成$TAB$A．
                        str = str.Replace("A．", "$TAB$A．").Replace("B．", "$TAB$B．").Replace("C．", "$TAB$C．").Replace("D．", "$TAB$D．").Replace("E．", "$TAB$E．").Replace("Ａ．", "$TAB$A．").Replace("Ｂ．", "$TAB$B．").Replace("Ｃ．", "$TAB$C．").Replace("Ｄ．", "$TAB$D．").Replace("Ｅ．", "$TAB$E．").Replace("A ．", "$TAB$A．").Replace("B ．", "$TAB$B．").Replace("C ．", "$TAB$C．").Replace("D ．", "$TAB$D．").Replace("E ．", "$TAB$E．");

                        str = str.Replace("A.", "$TAB$A．").Replace("B.", "$TAB$B．").Replace("C.", "$TAB$C．").Replace("D.", "$TAB$D．").Replace("E.", "$TAB$E．").Replace("Ａ.", "$TAB$A．").Replace("Ｂ.", "$TAB$B．").Replace("Ｃ.", "$TAB$C．").Replace("Ｄ.", "$TAB$D．").Replace("Ｅ.", "$TAB$E．");

                        strQ += str;
                    }
                    else
                    {
                        strQ += str;
                    }
                }
            }
            listOrig.Add(strQ);
            listOrig.ForEach(m =>
            {
                this.txtBoxTest1Result.Text += m + "\n";
            }
            );
        }

        /// <summary>
        /// 试卷处理步骤2  处理第一步加工过的试卷一
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            string str = this.txtBoxTest1Result.Text.Trim().Replace("\n", "");
            string[] arr = str.Split(new string[] { "$ZJ$" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sbAll = new StringBuilder();
            sbAll.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?><root>");
            for (int i = 0; i < arr.Length; i++)
            {
                //章节
                string[] arrZhang = arr[i].Split(new string[] { "$TM$" }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"<data name=""{0}"">", arrZhang[0]);
                sb.AppendFormat(@"<section type=""q"">");

                for (int timuNum = 1; timuNum < arrZhang.Length; timuNum++)
                { //每章
                    //<question name = "3．免疫对机体是（ ）" opa = "A．有害的" opb = "B．有利的" opc = "C．有利也有害" opd = "D．有利无害" ope = "E．正常条件下有利，异常条件下有害" />
                    string strtmp = arrZhang[timuNum].Replace("$TAB$A．", "\" opa = \"A．").Replace("$TAB$B．", "\" opb = \"B．").Replace("$TAB$C．", "\" opc = \"C．").Replace("$TAB$D．", "\" opd = \"D．").Replace("$TAB$E．", "\" ope = \"E．").Replace("$TAB$F．", "\" opf = \"F．").Replace("$TAB$G．", "\" opg = \"G．").Replace("$TAB$H．", "\" oph = \"H．").Replace("$TAB$I．", "\" opi = \"I．");
                    sb.AppendFormat(@"<question  type=""A1"" name = ""{0}""/>", strtmp);
                }
                sb.AppendFormat(@"</section>");
                sb.AppendFormat(@"</data>");
                sbAll.Append(sb.ToString());
            }
            sbAll.Append("</root>");
            this.txtBoxTest1Result.Text = sbAll.ToString();
        }
        #endregion

        #region 试卷一 处理摘出的题干
        private void button12_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                this.txtBoxTest1TiGan.Text = this.openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// 试卷一 提取出来的题干 第一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listOrig = new List<string>();

            string strQ = "";
            using (StreamReader sr = new StreamReader(this.txtBoxTest1TiGan.Text.Trim()))
            {
                while (sr.Peek() > -1)
                {
                    string str = sr.ReadLine().Trim();
                    //if (str.Contains("103.4kPa"))
                    {
                    }
                     if (Regex.IsMatch(str, @"^[\d]+[．.]", RegexOptions.Singleline))
                    {
                        listOrig.Add("$TM$" + str);
                    }
                }
            }
            listOrig.Add(strQ);
            listOrig.ForEach(m =>
            {
                this.txtBoxTest1TiGanResult.Text += m + "\n";
            }
            );
        }

        private void button11_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.txtBoxTest1Result.Text);
            //<root><data name="专业实务 "><section
            XmlNodeList nodeList = xmlDoc.SelectNodes(@"root/data/section/question");
            //      <question  type="A1" name = " 1．导致患者内源性感染的因素包括" opa = "A．医院内分布有大量的病原体" opb = "B．长期大量滥用抗生素" opc = "C．腹腔穿刺" opd = "D．消毒隔离措施不到位" ope = "E．医疗器械污染 "/>

            string str = this.txtBoxTest1TiGanResult.Text.Trim().Replace("\n", "");
            string[] arr = str.Split(new string[] { "$TM$" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < arr.Length; i++)
            {
                //多选题的题干
                int tiganNum = Convert.ToInt32(arr[i].Split('.')[0]);
                for (int nodeNum = 0; nodeNum < nodeList.Count; nodeNum++)
                {
                    string strName = nodeList[nodeNum].Attributes["name"].InnerText.Trim();
                    if (Regex.IsMatch(strName, string.Format(@"^({0})[．]", tiganNum), RegexOptions.Singleline))
                    {
                        nodeList[nodeNum].Attributes["type"].InnerText = "A3";
                        
                        //Create a new attribute.
                        XmlAttribute newAttr = xmlDoc.CreateAttribute("shareName");
                        newAttr.Value = arr[i].Replace(tiganNum.ToString() + ".", "");

                        //Create an attribute collection and add the new attribute 
                        //to the collection.
                        XmlAttributeCollection attrColl = xmlDoc.DocumentElement.Attributes;
                        attrColl.SetNamedItem(newAttr);

                        nodeList[nodeNum].Attributes.Append(newAttr);
                    }
                     
                }
            }
            this.txtBoxTest1TiGanResult.Text = xmlDoc.OuterXml;
        }
        #endregion 

        #region 替换试卷一的答案
        private void button14_Click(object sender, EventArgs e)
        {
            string str = this.txtBoxTest1Answer.Text;

            str =  str.Replace("\n","\t").Trim();
            str = str.Replace("\t",",");
            str = Regex.Replace(str,@"[\d]+[．.]","");
            
            //<section type="a">
            //    <answer name="A,C,E,A,B"/>
            //</section>

            this.txtBoxTest1Answer.Text = string.Format(@"<section type=""a""><answer name=""{0}""/></section>",str);

        }
        #endregion

        private void button18_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //从选择题 中的tab页面获取xml
            xmlDoc.LoadXml(this.txtBoxSelectResult.Text);
            //<root><data name="专业实务 "><section
            XmlNodeList nodeList = xmlDoc.SelectNodes(@"root/sectionTitle/testType/question");
            //      <question  type="A1" name = " 1．导致患者内源性感染的因素包括" opa = "A．医院内分布有大量的病原体" opb = "B．长期大量滥用抗生素" opc = "C．腹腔穿刺" opd = "D．消毒隔离措施不到位" ope = "E．医疗器械污染 "/>

            string str = this.txtBoxTest1TiGanResult.Text.Trim().Replace("\n", "");
            string[] arr = str.Split(new string[] { "$TM$" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < arr.Length; i++)
            {
                //多选题的题干
                int tiganNum = Convert.ToInt32(arr[i].Split(new char[] { '.' })[0]);
                for (int nodeNum = 0; nodeNum < nodeList.Count; nodeNum++)
                {
                    string strName = nodeList[nodeNum].Attributes["name"].InnerText.Trim();
                    if (Regex.IsMatch(strName, string.Format(@"^({0})[．.]", tiganNum), RegexOptions.Singleline))
                    {
                        nodeList[nodeNum].Attributes["type"].InnerText = "A3";
                        //去掉添加的序号文件前缀100
                        //string tmpName = nodeList[nodeNum].Attributes["name"].InnerText;
                        //nodeList[nodeNum].Attributes["name"].InnerText = tmpName.Trim().Substring(3);
                        //Create a new attribute.
                        XmlAttribute newAttr = xmlDoc.CreateAttribute("shareName");
                        newAttr.Value = arr[i].Replace(tiganNum.ToString() + ".", "");

                        //Create an attribute collection and add the new attribute 
                        //to the collection.
                        XmlAttributeCollection attrColl = xmlDoc.DocumentElement.Attributes;
                        attrColl.SetNamedItem(newAttr);

                        nodeList[nodeNum].Attributes.Append(newAttr);

                        /////////////////////////////

                    }

                }
            }
            this.txtBoxTest1TiGanResult.Text = xmlDoc.OuterXml;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.txtBoxTest1TiGanResult.Text);
            //<root><data name="专业实务 "><section
            XmlNodeList nodeList = xmlDoc.SelectNodes(@"root/sectionTitle");
            // <sectionTitle name="第1章 出入院护理技能 ">
            //    <testType type="c" name="选择题">
              //<question name = " 1..对高热患者的观察，不正确的是（ ）" opa = "A．.评估患者的心理状况                   " opb = "B．观察面色有无改变" opc = "C．.观察P、R、BP的变化                 " opd = "D．观察物理降温后的效果" ope = "E．.每日测量体温4次 "/>
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNodeList nodeListChild = nodeList[i].SelectNodes(@"testType/question");

                for (int questionCount = 0; questionCount < nodeListChild.Count; questionCount++)
                {
                    string strName = nodeListChild[questionCount].Attributes["name"].InnerText.Trim();
                    string [] arrNameList = strName.Split(new char[]{'.','．'});
                    if (Regex.IsMatch(arrNameList[0], string.Format(@"^({0})", arrNameList[0]), RegexOptions.Singleline))
                    {
                        //题目ID
                        int timuNum = questionCount + 1;
                        arrNameList[0] = timuNum.ToString();
                    }
                    strName = string.Join(".", arrNameList);
                    nodeListChild[questionCount].Attributes["name"].InnerText = strName;
                }
            }
            this.txtBoxTest1TiGanResult.Text = xmlDoc.OuterXml;


        }

        private void button16_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox3.Text = this.openFileDialog1.FileName;
            }

        }

        /// <summary>
        /// 问答题整理一
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            //            第1章  绪 论
            //1．儿科护理的护理特点分为       、      、       、        、      、      。
            //答案：沟通评估难、观察责任重、工作内容广、操作难度高、安全措施细、关怀照料勤
            //2．儿科护理学的任务？
            //答案：儿科护理的任务是根据小儿的生长发育、儿童保健、疾病防治的规范，运用护理专业理论和技术，提供“以小儿及其家庭为中心”的全方位整体护理，增强儿童体质，降低小儿发病率和病死率，保障和促进小儿身心健康。
            //整理成 
            /******
             * 练习题
             * 1．儿科护理的护理特点分为       、      、       、        、      、      。
             * 2．儿科护理学的任务？
             * 答案
             * 1.沟通评估难、观察责任重、工作内容广、操作难度高、安全措施细、关怀照料勤
             * 2.儿科护理的任务是根据小儿的生长发育、儿童保健、疾病防治的规范，运用护理专业理论和技术，提供“以小儿及其家庭为中心”的全方位整体护理，增强儿童体质，降低小儿发病率和病死率，保障和促进小儿身心健康。
             * ****/

            StringBuilder sb = new StringBuilder();
            List<string> listOrig = new List<string>();
            //问题
            StringBuilder sbQ = new StringBuilder();
            //答案
            StringBuilder sbA = new StringBuilder();
            using (StreamReader sr = new StreamReader(this.textBox3.Text.Trim()))
            {
                int strANum = 0;
                while (sr.Peek() > -1)
                {
                    string str = sr.ReadLine().Trim();
                    if(string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    //[第\d章]
                    if (Regex.IsMatch(str, @"第[\d]+章", RegexOptions.Singleline) )
                    {
                        sb.Append(sbQ.ToString() + sbA.ToString() + "\r\n");
                        sbQ.Remove(0, sbQ.Length);
                        sbA.Remove(0, sbA.Length);
                        strANum = 0;
                        
                        sbQ.AppendLine(str+"\r\n");
                        sbQ.AppendLine("练习题\r\n");
                    }
                    else if (str.Contains("答案"))
                    {
                        if (sbA.Length == 0)
                        {
                            sbA.AppendLine("答案");
                        }
                        strANum++;
                        sbA.AppendLine(strANum.ToString()+". "+str + "\r\n");
                    }
                    else
                    {
                        sbQ.AppendLine(str + "\r\n");
                    }
                }
                sb.Append(sbQ.ToString() + sbA.ToString() + "\r\n");

            }

           this.richTextBox7.Text = sb.ToString();
            

        }
    }
}
