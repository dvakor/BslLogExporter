ТекущееКоличество = Контекст.Записи.Количество();

ПредыдущиеЗаписи = Хранилище["Записи"];

Если ПредыдущиеЗаписи <> Неопределено Тогда
    
    ТекущееКоличество = ТекущееКоличество + ПредыдущиеЗаписи;
    
КонецЕсли;

Хранилище["Записи"] = ТекущееКоличество;



