namespace Promo.EverythingIsNew.DAL.Cbn.Dto
{
    public class UpdateResult
    {
        public bool status { get; set; }  // Boolean поле обязательно, Статус получения персональных данных. Если статус True. Тогда операция прошла успешно и ошибок не было обнаружено.
        public string description { get; set; }  // String(500)	Причина ошибки. Не пустое, если Status false. 
    }
}
