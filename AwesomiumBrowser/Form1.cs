using Awesomium.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AwesomiumBrowser
{
    public partial class Form1 : Form
    {
        ProgrammMethods pm;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pm = new ProgrammMethods(tabControl1, webSessionProvider1);
            pm.AddPages(tabControl1);
        }

        // Назад
        private void button_Back_Click(object sender, EventArgs e)
        {
            webControl().GoBack();
        }

        // Вперед
        private void button_Forward_Click(object sender, EventArgs e)
        {
            webControl().GoForward();
        }

        // Обновить
        private void button_Reload_Click(object sender, EventArgs e)
        {
            webControl().Reload(false);
        }

        // нажатие кнопки на сайте, заполнение поля и тп.
        private void button_Click_Click(object sender, EventArgs e)
        {
            pm.authorization(webControl());
        }

        // "Освободить память"
        private void button_Clear_Click(object sender, EventArgs e)
        {
            pm.TCA.ClearMemory(addressBox1);
        }

        // Добавить страницу(если вкладка с текстом  + )
        // -> выбирает эту страницу -> задает этой странице addressBox и задает текст addressBox(а) этой страницы
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            pm.TabControlSelecting(e, addressBox1);
        }

        //  ---------------------------> 
        // Данными 2-мя методами можно обойтись, если создать свой контрол, как тут [url]http://www.youtube.com/watch?v=DJu2ivQFooc[/url]
        // При клике по вкладке, определяет эту вкладку
        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pm.TCA.TabControlMouseDown(e);
        }

        //Закрыть вкладку
        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pm.TCA.CloseTab();
        }
        //  <---------------------------

        // Возвращает WebControl выбранной вкладки
        WebControl webControl()
        {
            return tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0] as WebControl;
        }
    }
}
