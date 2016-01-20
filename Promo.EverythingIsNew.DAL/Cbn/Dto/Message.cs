namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class Message
    {
        public string ctn { get; set; } // String(10)	Поле обязательно, Номер абонента в сети Билайн
        public string uid { get; set; } // String(20)	Поле обязательно, Идентификатор пользователя в системе, связанной с LP
        public string email { get; set; } // String(129) не обязательно, Email адрес абонента
        public string market { get; set; } // String(3) маркет-код

    }
}
