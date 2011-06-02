using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace fss_client
{
    class Diff
    {
        public static void diff(string fin0, string fin1, string fout0,
            string fout1, string fout2)
        {

            try
            {
                if (File.Exists(fout0))
                    File.Delete(fout0);
                if(File.Exists(fout1))
                    File.Delete(fout1);

                FileStream f_in_0 = new FileStream(fin0, FileMode.Open, FileAccess.Read);
                FileStream f_in_1 = new FileStream(fin1, FileMode.Open, FileAccess.Read);
                FileStream f_out_0 = new FileStream(fout0, FileMode.Create, FileAccess.Write);
                FileStream f_out_1 = new FileStream(fout1, FileMode.Create, FileAccess.Write);

                FileStream f_out_2;
                if (fout2 != string.Empty)
                    f_out_2 = new FileStream(fin0, FileMode.Create, FileAccess.Write);
                else
                    f_out_2 = null;

                long f_in_0_ln = get_line_num(f_in_0);
                long f_in_1_ln = get_line_num(f_in_1);

                bool[] flag = new bool[f_in_1_ln];
                for (int i = 0; i < f_in_1_ln; i++)
                    flag[i] = false;

                int j, k;
                f_in_0.Position = 0;
                for (j = 0; j < f_in_0_ln; j++)
                {
                    string str0 = get_line(f_in_0);

                    f_in_1.Position = 0;
                    for (k = 0; k < f_in_1_ln; k++)
                    {
                        string str1 = get_line(f_in_1);
                        if (flag[k])
                            continue;

                        if (string.Compare(str0, str1) == 0)
                        {
                            flag[k] = true;
                            break;
                        }
                    }

                    if (k == f_in_1_ln)
                    {
                        write_line_num(j + 1, f_out_0);
                    }
                }

                for (int m = 0; m < f_in_1_ln; m++)
                {
                    if (!flag[m])
                    {
                        write_line_num(m + 1, f_out_1);
                    }
                    else
                        if (f_out_2 != null)
                            write_line_num(m + 1, f_out_2);
                }

                f_in_0.Close();
                f_in_1.Close();
                f_out_0.Flush(); f_out_0.Close();
                f_out_1.Flush(); f_out_1.Close();
                if (f_out_2 != null)
                {
                    f_out_2.Flush();
                    f_out_2.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        private static long get_line_num(FileStream fs)
        {
            long pos = fs.Position;
            int c;
            long num = 0;

            for (int i = 0; i < fs.Length; i++)
            {
                c = fs.ReadByte();
                if (c == '\n')
                    num++;
            }

            fs.Position = pos;

            return num;

        }

        // get_line via fs's internal position
        public static string get_line(FileStream fs)
        {
            if (fs.Position == fs.Length)
                return string.Empty;

            byte[] record = new byte[Net.MAX_PATH_LEN];
            int c;
            int count = 0;

            c = fs.ReadByte();
            while (c != '\n')
            {
                record[count++] = (byte)c;


                c = fs.ReadByte();
            }
            return Encoding.Default.GetString(record, 0, count);

        }

        public static string get_line_via_linenum(string fullpath, long linenum)
        {
            if (linenum <= 0)
                return string.Empty;
            try
            {
                FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
                string record =  get_line_via_linenum(fs, linenum);
                fs.Close();
                return record;
                     
            }
            catch (Exception e)
            {
                Log.logon(e.ToString());
            }

            return string.Empty;
        }

        public static string get_line_via_linenum(FileStream fs, long linenum)
        {
            if (fs.Position == fs.Length || linenum > fs.Length)
                return string.Empty;

            int c = 0;
            int new_line_count = 0;
            bool found = false;

            fs.Position = 0;
            if (linenum == 1)
                return get_line(fs);
            else
            {
                int i;
                for (i = 0; i < fs.Length; i++)
                {
                    c = fs.ReadByte();
                    if (c == '\n')
                    {
                        if (++new_line_count == linenum - 1)
                        {
                            found = true;
                            break;
                        }
                    }
                }


                if (!found)
                    return string.Empty;
                else
                    return get_line(fs);

            }

        }

        public static long search_line(string fullpath, string target)
        {
            long rv = -1, i = 0, total_linenum = 0;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
                total_linenum = get_line_num(fs);

                for (i = 1; i <= total_linenum; i++)
                {
                    if (get_line_via_linenum(fs, i) == target)
                    {
                        rv = i;
                        break;
                    }
                }

                if (i == (total_linenum + 1))
                    rv = -1;

            }
            catch (Exception e)
            {
                Log.logon("@ Diff.cs, search_line, maybe invoked by reused_file" + e.ToString());

            }

            if (fs != null)
                fs.Close();
            return rv;                

        }

        private static void write_line_num(int num, FileStream fs)
        {
            string num_str = Convert.ToString(num);
            byte[] num_byte = Encoding.Default.GetBytes(num_str);
            fs.Write(num_byte, 0, num_byte.Length);
            fs.WriteByte((byte)'\n');

        }
    }
}
