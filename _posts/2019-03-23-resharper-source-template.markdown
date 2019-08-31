---
title: "Фичи решарпера. ReSharper Source Template"
layout: post
date: 2019-03-23 12:00
image: /assets/images/2019/resharper-source-template.gif
tag:
- resharper
- xunit
---
Одна из моих любимых возможностей решарпера - Source Template. Я называю ее inverted flow.
Когда вы сначала пишете выражение, а потом присваиваете его переменной. Или сначала фильтруете коллекцию, а потом дописываете foreach и ваша получаете foreach структуру.

![resharper source template](/assets/images/2019/resharper-source-template.gif)

Решарпер предоставляет [список темплейтов по умолчанию](https://www.jetbrains.com/help/rider/Postfix_Templates.html#list). Также есть возможность создавать темплейты самому.
Source template - это extension method с атрибутом `[SourceTemplate]`. SourceTemplateAttribute находится в nuget пакете JetBrains.Annotations.

Чтобы написать свой темплейт - установите JetBrains.Annotations в свой проект. Создайте статический класс. Дальше добавьте extension метод с атрибутом SourceTemplate. Все содержимое этого метода будет преобразовано в темплейт.

```charp
public static class XUnitTemplate
{
    [SourceTemplate]
    public static void assertTrue(this bool obj)
    {
        //$ Assert.True(obj);$END$
    }
}
```
![assert true example](/assets/images/2019/assertTrue.gif)
Детальнее про Source Template можно почитать на [официальном сайте](https://www.jetbrains.com/help/rider/Source_Templates.html)