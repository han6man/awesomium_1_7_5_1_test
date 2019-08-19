using System.Windows.Forms;
using Awesomium.Windows.Forms;

namespace AwesomiumBrowser
{
    class ProgrammMethods
    {
        NavigationOnWebControl NOW;
        public TabControlAwesomium TCA;

        public ProgrammMethods(TabControl tabControl, WebSessionProvider webSessionProvider)
        {
            NOW = new NavigationOnWebControl();
            TCA = new TabControlAwesomium(tabControl, webSessionProvider);
        }

        public void AddPages(TabControl tabControl) // При загрузки формы добавляет 2 вкладки на TabControl
        {
            tabControl.TabPages.Insert(1, "   +");
            TCA.AddPage();
        }

        public void authorization(WebControl webControl)
        {
            //Yandex.ru
            NOW.WriteInField(webControl, NavigationOnWebControl.GetElementBy.Name, "login", "логин");
            NOW.WriteInField(webControl, NavigationOnWebControl.GetElementBy.Name, "passwd", "парол123");
            NOW.PressButton(webControl, NavigationOnWebControl.GetElementBy.ClassName, "button auth__login-button button_size_s button_theme_normal i-bem button_js_inited");

            //https://signup.live.com/signup?wa=wsignin1.0&rpsnv=12&ct=1438670714&rver=6.0.5276.0&wp=MCMBI&wlcxt=MSDN%24MSDN%24MSDN&wreply=https%3a%2f%2fmsdn.microsoft.com%2fru-ru%2fdn308572&id=254354&bk=1438670732&uiflavor=web&uaid=f38a6afb74524c22a1a76f8a06aca926&mkt=RU-RU&lc=1049&lic=1
            //NOW.SelectComboBoxValue(webControl, NavigationOnWebControl.GetElementBy.Id, "BirthDay", 8);
        }

        public void TabControlSelecting(TabControlCancelEventArgs e, AddressBox addressBox) //Добавляет страницу -> выбираем или выбирает эту страницу -> задает этой странице addressBox и задает текст addressBox(а) этой страницы
        {

            if (e.TabPage.Text == "   +")
            {
                TCA.AddPage();
            }
            else
            {
                TCA.SetWebControlInAddressBox(addressBox);
                TCA.SetTextAddressBox(addressBox);
            }

        }




    }
}