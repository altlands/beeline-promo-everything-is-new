namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class Update
    {
        public string ctn { get; set; } // String(10)  поле не обязательно, Номер абонента в сети Билайн
        public string name { get; set; } // String(50)  не обязательно, Имя абонента
        public string surname { get; set; } // String(100) не обязательно, Фамилия абонента
        public string region { get; set; } // String(100)  не обязательно, Город абонента
        public string email { get; set; } // String(129) не обязательно Email адрес абонента
        public string birth_date { get; set; } // Date    не обязательно, Дата рождения абонента
        public bool email_unsubscribe { get; set; } // boolean не обязательно, Отписка абонента от рассылки. Если true, тогда стоит отказ от рассылки.
    }
}
