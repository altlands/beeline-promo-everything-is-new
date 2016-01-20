namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class Status
    {
        public string ctn { get; set; } // String(10)	Обязательно:Да	Номер абонента в сети Билайн
        public string uid { get; set; } // String(20)	Идентификатор пользователя в системе, связанной с LP
    }
}
