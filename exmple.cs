public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MiddleName { get; set; }
    public string Surname { get; set; }
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class UserSummary
{
    public string FullName { get; set; }
    public int Age { get; set; }

    public bool IsAdult { get; set; }
}

public abstract class Base
{
    public UserSummary Convert(User user)
    {
        var age = GetAge(user);
        return new UserSummary()
        {
            Age = age,
            FullName = GetFullName(user),
            IsAdult = IsAdult(age)
        };
    }

    private bool IsAdult(int age)
    {
        return age >= 18;
    }

    protected abstract string GetFullName(User user);


    protected abstract int GetAge(User user);
}

public class Imp1 : Base
{
    protected override string GetFullName(User user)
    {
        return $"{user.Name} {user.MiddleName} {user.Surname}";
    }

    protected override int GetAge(User user)
    {
        return user.Age.Value;
    }
}

public class Imp2 : Base
{
    protected override string GetFullName(User user)
    {
        return $"{user.Name} {user.Surname}";
    }

    protected override int GetAge(User user)
    {
        return (int)(DateTime.Now.Subtract(user.BirthDate.Value).TotalDays / 365);
    }
}

Base converter = user.Id>100? new Imp1() : new Imp2();
var userSummary = converter.Convert(user);
