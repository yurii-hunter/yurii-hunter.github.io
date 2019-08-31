---
title: "AutoFixture и подготовка тестовых данных"
layout: post
date: 2019-03-30 12:00
image: /assets/images/2019/autofixture.jpg
headerImage: false
tag:
- autofixture
star: false
category: blog
author: yurii-hunter
description: "AutoFixture и подготовка тестовых данных"
---
Уже много лет следую концепции, что тесты сами подготавливают себе данные. Было много подходов хранения: в json файлах, в xml файлах, в коде, в SpecFlow файлах. У каждого подхода свои преимущества и недостатки.

Проблема хранения данных в файлах состоит в том, что внешние файлы сложно поддерживать. Нельзя нажать Ctrl+B и сразу же перейти к полю в json файле. Добавление или удаление полей в модели никак не отразится в момент компиляции, если разработчик забыл обновить файлы.

Можно еще писать билдеры для моделей. Этот вариант более безопасный. Проблема в том, что если модель сложная, то билдеров придется написать много. 

Как улучшенную альтернативу билдерам использую [AutoFixture](https://github.com/AutoFixture/AutoFixture). Библиотека создает сложные объекты и заполняет их случайными данными. AutoFixture умеет присваивать отдельным полям конкретные значения, а  остальные будут сгенерированы в зависимости от типа поля.

```csharp
[Fact]
public void IntroductoryTest()
{
    // Arrange
    Fixture fixture = new Fixture();

    int expectedNumber = fixture.Create<int>();
    MyClass sut = fixture.Create<MyClass>();
    // Act
    int result = sut.Echo(expectedNumber);
    // Assert
    Assert.Equal(expectedNumber, result);
}
```

Первый способ создания объектов, которые предоставляет Fixture - это вызов метода Create на экземпляре Fixture. В этом случае будет сгенерирован объект со случайными значениями во всех полях. Но что если нужно для некоторых полей задать конкретное значение? Для таких случаев можно кастомизировать инициализацию некоторых полей. 

```csharp
Fixture fixture = new Fixture();
fixture.Customize<MyClass>(composer =>
    composer
        .With(x => x.IsRemoved, false)
        .With(x => x.Name, "Class Name")
);
```

В этом случае у созданных объектов поле Name будет со значением "Class Name", поле IsRemoved со значением false, а остальные поля будут заполнены случайно.

Второй способ - использовать метод Build. Можно кастомизировать поля и сразу же создать экземпляр класса.

```csharp
Fixture fixture = new Fixture();
var instance = fixture.Build<MyClass>()
    .With(x=>x.Name, "My Name")
    .With(x=>x.Type, "Test")
    .Create();
```

В этом случае кастомизация не сохраняется в экземпляре fixture и новые объекты не будут получать значение "My Name" в поле Name.

Этих двух подходов хватает в 80% случаев. Но было бы здорово преднастроить экземпляр fixture, а потом переопределить часть полей с помощью Build и создать новый объект. Проблема в том, что метод Build не учитывает настройки fixture, которые заданы методом Customize.

Скачав исходники и посмотрев что там внутри, я написал в extension метода, которые решают эту задачу.

```csharp
public static class FixtureExtension
{
    public static ICustomizationComposer<T> For<T>(this Fixture fixture)
    {
        return new CompositeNodeComposer<T>(fixture.Graph());
    }
    
    public static ISpecimenBuilderNode Graph(this Fixture fixture)
    {
        var fieldInfo = fixture.GetType().GetField("graph", BindingFlags.Instance | BindingFlags.NonPublic);
        return (ISpecimenBuilderNode)fieldInfo.GetValue(fixture);
    }
}
```

Теперь можно написать `fixture.For<MyClass>().With(x=>x.Name, "My Name")` сохранить изначальные настройки fixture, а переопределить только одно поле Name и получить новый объект класса MyClass.

```csharp
Fixture fixture = new Fixture();
fixture.Customize<MyClass>(composer =>
    composer
        .With(x => x.IsRemoved, false)
        .With(x => x.Name, "Class Name")
);
var instance = fixture.For<MyClass>()
    .With(x=>x.Name, "My Name")
    .Create();
```

Объект instance будет создан со значением false для поля IsRemoved, со значением "My Name" для поля Name и остальные поля будут заполнены случайным образом.

Такой вариант меня устраивает. Я настраиваю Fixture более реальными данными, а потом переопределяю некоторые поля в зависимости от теста. Количество кода сводится к минимуму, данные читабельные, выглядит это хорошо. Советую попробовать этот подход в вашем следующем тесте.