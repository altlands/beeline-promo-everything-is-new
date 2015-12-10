namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class MessageResult
    {
        public bool is_message_sent { get; set; } // отправлено ли сообщение
        public string description { get; set; } // Причина ошибки.Не пустое, если is_message_sent false. 
        public string code { get; set; } // Промо-код для участия в акции.Формируется, если ни CTN, ни UID не участвовали ранее.И высылается прошлый промо код, если связка CTN+UID участвовала ранее.Если CTN и UID участвовали ранее в другой связке, то промо код не высылается.
        public string is_change_email_available { get; set; } // Признак разрешающий смену емайла
        
    }
}
