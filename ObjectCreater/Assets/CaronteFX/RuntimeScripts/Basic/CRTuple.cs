using UnityEngine;
using System;
using System.Collections;

namespace CaronteFX
{
  [Serializable]
  public class Tuple2<T1, T2>
  {
    [SerializeField]
    T1 first_;
    [SerializeField]
    T2 second_;

    public T1 First  { get { return first_; }  protected set { first_  = value; } }
    public T2 Second { get { return second_; } protected set { second_ = value; } }

    internal Tuple2(T1 first, T2 second)
    {
      First = first;
      Second = second;
    }
  }

  public static class Tuple2
  {
    public static Tuple2<T1, T2> New<T1, T2>(T1 first, T2 second)
    {
      var tuple = new Tuple2<T1, T2>(first, second);
      return tuple;
    }
  }

  [Serializable]
  public class Tuple3<T1, T2, T3>
  {
    [SerializeField]
    T1 first_;
    [SerializeField]
    T2 second_;
    [SerializeField]
    T3 third_;

    public T1 First  { get { return first_; }  protected set { first_ = value; } }
    public T2 Second { get { return second_; } protected set { second_ = value; } }
    public T3 Third  { get { return third_; }  protected set { third_ = value; } }
    internal Tuple3(T1 first, T2 second, T3 third)
    {
      First  = first;
      Second = second;
      Third  = third;
    }
  }

  public static class Tuple3
  {
    public static Tuple3<T1, T2, T3> New<T1, T2, T3>(T1 first, T2 second, T3 third)
    {
      var tuple = new Tuple3<T1, T2, T3>(first, second, third);
      return tuple;
    }
  }

  [Serializable]
  public class Tuple4<T1, T2, T3, T4>
  {
    [SerializeField]
    T1 first_;
    [SerializeField]
    T2 second_;
    [SerializeField]
    T3 third_;
    [SerializeField]
    T4 fourth_;

    public T1 First  { get { return first_; }  protected set { first_ = value; } }
    public T2 Second { get { return second_; } protected set { second_ = value; } }
    public T3 Third  { get { return third_; }  protected set { third_ = value; } }
    public T4 Fourth { get { return fourth_; } protected set { fourth_ = value; } }

    internal Tuple4(T1 first, T2 second, T3 third, T4 fourth)
    {
      First  = first;
      Second = second;
      Third  = third;
      Fourth = fourth;
    }
  }

  public static class Tuple4
  {
    public static Tuple4<T1, T2, T3, T4> New<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
    {
      var tuple = new Tuple4<T1, T2, T3, T4>(first, second, third, fourth);
      return tuple;
    }
  }

  [Serializable]
  public class Tuple5<T1, T2, T3, T4, T5>
  {
    [SerializeField]
    T1 first_;
    [SerializeField]
    T2 second_;
    [SerializeField]
    T3 third_;
    [SerializeField]
    T4 fourth_;
    [SerializeField]
    T5 fifth_;

    public T1 First  { get { return first_; }  protected set { first_  = value; } }
    public T2 Second { get { return second_; } protected set { second_ = value; } }
    public T3 Third  { get { return third_; }  protected set { third_  = value; } }
    public T4 Fourth { get { return fourth_; } protected set { fourth_ = value; } }
    public T5 Fifth  { get { return fifth_; }  protected set { fifth_  = value; } }

    internal Tuple5(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
    {
      First  = first;
      Second = second;
      Third  = third;
      Fourth = fourth;
      Fifth  = fifth;
    }
  }

  public static class Tuple5
  {
    public static Tuple5<T1, T2, T3, T4, T5> New<T1, T2, T3, T4, T5>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
    {
      var tuple = new Tuple5<T1, T2, T3, T4, T5>(first, second, third, fourth, fifth);
      return tuple;
    }
  }

}
