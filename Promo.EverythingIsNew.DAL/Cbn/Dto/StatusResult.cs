namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class StatusResult
    {
        public bool is_beeline_subscriber { get; set; } // принадлежит ли номер абоненту билайн
        public bool is_used_uid { get; set; } // участвовал ли данный аккаунт в акции
        public bool is_used_ctn { get; set; } // участвовал ли данный абонент в акции
        public bool Is_used_uid_with_ctn { get; set; } //Присутствует если UID и CTN уже использовались. Если в одной связке, то вернется true, если связка в прошлый раз была другой, то false

    }
}
