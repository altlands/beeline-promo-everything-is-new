using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; } // 5.1	Имя, Фамилия(из ВК);
        public string CTN { get; set; }  // 5.2	CTN (пустое поле);
        public string Email { get; set; }  // 5.3	Email (если есть, из ВК);
        public DateTime? Birthday { get; set; }  // 5.4	Дата рождения (из ВК);
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string SelectMyCity { get; set; }  // 5.5	Город (из ВК или автоопределенный) 5.5.1	Если указано несколько городов, то берется первый из списка
        public string Captcha { get; set; }
        public bool IsMailingAgree { get; set; }  // Согласие на получение рассылки
        public bool IsPersonalDataAgree { get; set; }  // Согласие на обработку персональных данных
        public string Academy { get; set; }  // Вуз
        public string Uid { get; set; }
        public string MarketCode { get; set; }
        public string Soc { get; set; }
    }

    /*
    10	LP осуществляет проверку корректного заполнения полей формы. При выявлении некорректного заполнения LP показывает сообщение у соответствующего поля:
        10.1	Поле «Город» не может быть пустым;
        10.2	Номер телефона (CTN) (не может быть пустым) – только цифры (количество? Формат?)
        10.3	Имя и Фамилия (не может быть пустым) – не более 100 символов
        10.4	Дата рождения (не может быть пустым) – в формате ДД.ММ.ГГГГ;
        10.5	Email (необязательное) – если заполнено, то только английские символы и знаки и формат *@*.*

    */
}