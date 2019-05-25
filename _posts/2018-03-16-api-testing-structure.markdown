---
title: "REST API тестирование. Организация проекта"
layout: post
date: 2018-03-16 12:00
image: /assets/images/api_testing_featured.jpg
headerImage: false
tag:
- resharper
- api
star: false
category: blog
author: yurii-hunter
description: REST API тестирование. Организация проекта
---
Наверное каждый, кто занимался UI тестированием приложений, слышал о Page Object и Page Factory патернах проектирования. И, уж точно, каждый из них знает в чем их преимущество. Но когда дело доходит до API, то тут начинается импровизация. Кто как придумал, кто как смог... кто-то формирует запросы в файлах, кто-то в классас, а кто-то и в тест методах не брезгует. Ниже я опишу вам как решал этот вопрос я у себя на проекте.

## А давайте делать как в Page Object pattern
Ну правда, давайте разделим наши запросы на Resources и Actions по аналогии с Pages и Steps. В Resouces будут находиться статические данные к endpoint'ам, а в Actions мы добавим динамические данные и выполним сам запрос. Статические данные будем хранить в атрибутах Request и Format. Request содержит метод запроса и url к endpoint, а Format определяет формат данных которые будут отправлены или получены. Опишем условный Users ресурс нашего приложения со стандартным набором CRUD операций.
```csharp
internal class UserResource
{
    [Request(Method.GET, "users")]
    public Endpoint GetAll { get; set; }

    [Request(Method.GET, "users/{userId}")]
    public Endpoint Get { get; set; }

    [Request(Method.POST, "users")]
    [Format(DataFormat.Json)]
    public Endpoint Add { get; set; }

    [Request(Method.PUT, "users/{userId}")]
    [Format(DataFormat.Json)]
    public Endpoint Update { get; set; }

    [Request(Method.DELETE, "users/{userId}")]
    public Endpoint Delete { get; set; }
}
```
А теперь посмотрим как выглядит Actions для UsersResource
```csharp
public class UsersActions
{
    private UsersResource UsersResource => Resource.Get<UsersResource>();

    public IEnumerable<User> GetUsers()
    {
        return UsersResource
            .GetAll
            .Execute<IEnumerable<User>>();
    }

    public User GetUserById(long userId)
    {
        return UsersResource
            .Get
            .WithUrlSegment("userId", userId)
            .Execute<User>();
    }

    public void AddUser(User user)
    {
        UsersResource
            .Add
            .WithData(user)
            .Execute();
    }

    public void UpdateUser(long userId, User user)
    {
        UsersResource
            .Update
            .WithUrlSegment("userId", userId)
            .WithData(user)
            .Execute();
    }

    public void DeleteUser(long userId)
    {
        UsersResource
            .Delete
            .WithUrlSegment("userId", userId)
            .Execute();
    }
}
```
Выглядит просто и лаконично, правда? Но пока не понятно, что за тип такой Endpoint. Endpoint - это класс билдер зпроса. У себя на проекте я использую RestSharp, так что Endpoint - это обертка над RestRequest, но всегда можно использовать что-то другое.
```csharp
public class Endpoint
{
    private readonly RestRequest _request = new RestRequest();
    private readonly RestClient _restClient;
    public Endpoint(RestClient client)
    {
        _restClient = client;
    }

    public Endpoint WithMethod(Method method)
    {
        _request.Method = method.Convert();
        return this;
    }

    public Endpoint WithResource(string resource)
    {
        _request.Resource = resource;
        return this;
    }

    public Endpoint WithDataFormat(DataFormat format)
    {
        _request.RequestFormat = format.Convert();
        return this;
    }

    public Endpoint WithUrlSegment(string name, object value)
    {
        _request.AddUrlSegment(name, value.ToString());
        return this;
    }

    public Endpoint WithParameter(string name, object value)
    {
        _request.AddQueryParameter(name, value.ToString());
        return this;
    }

    public Endpoint WithData(object body)
    {
        _request.AddBody(body);
        return this;
    }

    public void Execute()
    {
        _restClient.Execute(_request);
    }

    internal T Execute<T>()
    {
        var content = _restClient.Execute(_request).Content;
        return JsonConvert.DeserializeObject<T>(content);
    }
}
```
Такой подход позволяет построить простоую в поддержке и использовании структуру проекта. Все изменения происходят в одном месте и нет повторного дублироввания кода. Сам проект можно посмотреть и скачать на моем профиле GitHub.
