using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace parser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        private void textbox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                dataGridView1.Rows.Clear();
                string gelen = textBox1.Text;
                string ebcdic = "";
                string kacByte = "", pds = "", karsilik = "";
                int sira = 0, pdsSayac = 0, karsilikSayac = 0, alinanSayac = 0, kacByteInt = 0, icerikSayac = 0;
                string gelen2 = "404040400066F9F6F0F00106201607240000000538492016082400000005384920160924000000053849201610240000000538492016112400000005384920161224000000053849F3F5F956F05100000032309400000001924600000000096200000000288600017515000500";
                //string gelen = "40404040F0F8F4F6F0F000010300F1F0F9F4F0F2000000000430F1F0F9F4F0F3000000000000F0F7F9F4F0F5090293F2F0F9F9F0F0F5C6C4F2F6F1F9F5F2C5C1F4F1F4F5F5F0F6F9F9F0F13105F2F0F9F9F0F20000123456780000FFFFFFFF167632F0F0F6F9F9F0F3002D";
                List<char> alinan = new List<char>();
                List<string> pdsList = new List<string>();
                ConcurrentDictionary<string, List<string>> ltv = new ConcurrentDictionary<string, List<string>>();// pds değerinin karsilik geldiği 0 indisi toplam değer,1den itibaren diğer indisler ozellikleri
                ConcurrentDictionary<string, List<string>> ozellikler = new ConcurrentDictionary<string, List<string>>();
                List<string> bosList = new List<string>();


                for (sira = 0; sira < gelen.Count(); sira++)
                {
                    alinan.Add(gelen[sira]);


                    if (sira == 7)//EBCDIC
                    {
                        ebcdic = string.Join("", alinan.ToArray());
                        alinan.Clear();


                    }
                    else if (sira > 7)
                    {

                        if (alinanSayac == 1 || alinanSayac == 3)
                        {
                            kacByte += alinan[alinanSayac];
                            kacByteInt = int.Parse(kacByte);
                        }
                        else if (alinanSayac == 5 || alinanSayac == 7 || alinanSayac == 9 || alinanSayac == 11)
                        {
                            pds += alinan[alinanSayac];
                            pdsSayac++;

                            if (pdsList.Equals("9600"))//9600 pdste BCD ve kalan datanın alınması
                            {
                                for (int j = sira; j < gelen.Count(); j++)
                                {
                                    alinan.Add(gelen[sira]);
                                    karsilik += alinan[sira];
                                }

                                Console.WriteLine("ebcid " + ebcdic);
                                Console.WriteLine("byte " + kacByte);
                                Console.WriteLine("pds " + pds);
                                Console.WriteLine("karsilik " + karsilik);
                                Console.WriteLine("---------------");
                                break;
                            }
                        }
                        else if (alinanSayac >= 12)
                        {

                            if (karsilikSayac < kacByteInt * 2 - 8)//kaç bit karşılık sayacağından pdste zaten saydığını çıkarıp karşılığı buluyor
                            {
                                karsilik += alinan[alinanSayac];
                                karsilikSayac++;
                            }
                            else
                            {
                                List<string> listKarsilik = karsilik.Split(',').ToList();
                                ltv[pds] = listKarsilik;

                                Console.WriteLine("ebcid " + ebcdic);
                                Console.WriteLine("byte " + kacByte);
                                Console.WriteLine("pds " + pds);
                                Console.WriteLine("karsilik " + karsilik);
                                Console.WriteLine();
                                Console.WriteLine("---------------");
                                karsilikSayac = 0;
                                karsilik = "";
                                pdsSayac = 0;
                                kacByte = "";
                                pds = "";
                                alinanSayac = 0;
                                alinan.Clear();
                                alinan.Add(gelen[sira]);
                                icerikSayac++;
                            }


                        }
                        alinanSayac++;


                    }


                }
                List<string> listeKarsilik = karsilik.Split(',').ToList();
                ltv[pds] = listeKarsilik;
                Console.WriteLine("ebcid " + ebcdic);
                Console.WriteLine("byte " + kacByte);
                Console.WriteLine("pds " + pds);
                Console.WriteLine("karsilik " + karsilik);
                Console.WriteLine("---------------");

                string ozellik = "";
                foreach (KeyValuePair<string, List<string>> entry in ltv)// maplanan pds ve karşılıkları
                {
                    Console.WriteLine("pds " + entry.Key + " - value " + entry.Value.First());

                    for (int i = 0; i < entry.Value.First().Count(); i++)//pds değeri kadar dönecek değeri parse edecek 2li 2li
                    {

                        if (i % 2 == 0 && i != 0)
                        {
                            ltv[entry.Key].Add(ozellik);
                            ozellik = "";
                            ozellik += entry.Value[0].ElementAt(i);


                        }
                        else
                        {

                            ozellik += entry.Value[0].ElementAt(i);

                            if (i == entry.Value.First().Count() - 1)
                            {
                                ltv[entry.Key].Add(ozellik);
                                ozellik = "";
                            }


                        }


                    }
                    dataGridView1.Rows.Add(entry.Key, entry.Value.First());//grid işlemi
                }

                int byteSayac = 0;
                foreach (KeyValuePair<string, List<string>> entry in ltv)// ozellikler ekleniyor
                {
                    //Console.WriteLine("pds " + entry.Key + " - value " + entry.Value.First());

                    for (int i = 0; i <= entry.Value.First().Count(); i++)//
                    {

                        if (entry.Key.Equals("4600"))
                        {

                            if (i % 2 == 0 && i != 0)
                            {

                                if (byteSayac == 0)
                                {
                                    if (entry.Value[byteSayac + 1] == "00")
                                    {
                                        bosList.Add("Online");
                                    }
                                    else if (entry.Value[byteSayac + 1] == "01")
                                    {
                                        bosList.Add("Offline");
                                    }
                                }

                                if (byteSayac == 1)
                                {
                                    if (entry.Value[byteSayac + 1] == "00")
                                    {
                                        bosList.Add("Tanımsız");
                                    }
                                    else if (entry.Value[byteSayac + 1] == "01")
                                    {
                                        bosList.Add("Başarılı");
                                    }
                                    else if (entry.Value[byteSayac + 1] == "02")
                                    {
                                        bosList.Add("Başarısız");
                                    }

                                }
                                if (byteSayac == 2)
                                {
                                    bosList.Add("Sabit Tanımı Yok");
                                }

                                if (byteSayac == 3)
                                {
                                    if (entry.Value[byteSayac + 1] == "00")
                                    {
                                        bosList.Add("Tanımsız");
                                    }
                                    else if (entry.Value[byteSayac + 1] == "01")
                                    {
                                        bosList.Add("Block");
                                    }
                                    else if (entry.Value[byteSayac + 1] == "02")
                                    {
                                        bosList.Add("By-Pass");
                                    }
                                    byteSayac = 0;
                                    ozellikler[entry.Key] = bosList;
                                    //dataGridView1.Rows[i%8].Cells[2].Value = string.Join<string>(" ", bosList);
                                }
                                byteSayac++;
                                
                            }
                            
                            
                        }
                        else if (entry.Key.Equals("9600"))
                        {

                            if (i % 2 == 0 && i != 0)
                            {
                                switch (byteSayac)
                                {
                                    case 0:
                                        if (entry.Value[byteSayac + 1] == "00")
                                        {
                                            bosList.Add("Ekspress Limit");
                                        }
                                        else if (entry.Value[byteSayac + 1] == "01")
                                        {
                                            bosList.Add("Sabit Ödemeli");
                                        }
                                        byteSayac = 0;
                                        //dataGridView1.Rows[i % 8].Cells[2].Value = string.Join<string>(" ", bosList);
                                        break;
                                    default: byteSayac = 0; break;
                                }
                                byteSayac++;

                            }
                            ozellikler[entry.Key] = bosList;
                           
                        }
                        else
                        {
                            if (i % 2 == 0 && i != 0)
                            {
                                ozellikler[entry.Key] = "Boş".Split(',').ToList();
                                
                            }
                            //dataGridView1.Rows[i % 8].Cells[2].Value = string.Join<string>(" ", bosList);
                        }
                        
                    }
                }
                ozellikSayac = 0;
                foreach (KeyValuePair<string, List<string>> entry in ozellikler)
                {

                    dataGridView1.Rows[ozellikSayac].Cells[2].Value = string.Join<string>(" ", entry.Value);
                    ozellikSayac++;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public int ozellikSayac { get; set; }
    }
}
