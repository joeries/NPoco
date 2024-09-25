using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPoco;

namespace NPoco.Tester
{
    /// <summary>
    /// 
    /// </summary>
    [PrimaryKey(primaryKey: "captcha_id")]
    public class base_captcha
    {
        /// <summary>
        /// 随机码编号
        /// </summary>		
        public int? captcha_id
        {
            get;
            set;
        }

        /// <summary>
        /// 随机码GUID
        /// </summary>		
        public string captcha_guid
        {
            get;
            set;
        }

        /// <summary>
        /// 随机码文本
        /// </summary>		
        public string captcha_text
        {
            get;
            set;
        }

        /// <summary>
        /// 随机码生成时间
        /// </summary>		
        public DateTime? captcha_time
        {
            get;
            set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //IDatabase db = new Database(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\test.mdb;Persist Security Info=False", "System.Data.OleDb");
            //Page<base_captcha> pageCaptcha = db.Page<base_captcha>(2, 10, "select * from base_captcha where captcha_id=0 order by captcha_id desc");

            IDatabase db = new Database(@"host=127.0.0.1;port=3306;database=test;uid=test;pwd=123456;Convert Zero Datetime=True;", "MySql.Data.MySqlClient");
            Page<base_captcha> pageCaptcha = db.Page<base_captcha>(2, 10, "select * from base_captcha where captcha_id=0 order by captcha_id desc");
        }
    }
}