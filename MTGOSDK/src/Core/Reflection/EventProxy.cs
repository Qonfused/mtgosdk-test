/** @file
  Copyright (c) 2023, Cory Bennett. All rights reserved.
  SPDX-License-Identifier: Apache-2.0
**/

using System.Linq;


namespace MTGOSDK.Core.Reflection;

/// <summary>
/// A wrapper for dynamic objects that implement events at runtime.
/// </summary>
/// <typeparam name="I">The instance type of the sender to wrap.</typeparam>
/// <typeparam name="T">The type of the event arguments to wrap.</typeparam>
/// <remarks>
/// This class exposes a "+" and "-" operator overload for subscribing and
/// unsubscribing to events. This allows for a more natural syntax for event
/// subscription and unsubscription.
/// </remarks>
public sealed class EventProxy<I, T>(dynamic @ref, string name) : DLRWrapper<I>
    where I : class
    where T : class
{
  internal override dynamic obj => @ref;

  internal void EventSubscribe(string eventName, Delegate callback) =>
    @ro.EventSubscribe(eventName, callback);

  internal void EventUnsubscribe(string eventName, Delegate callback) =>
    @ro.EventUnsubscribe(eventName, callback);

  private Delegate ProxyTypedDelegate(Delegate c) =>
    new Action<dynamic, dynamic>((dynamic obj, dynamic args) =>
    {
      switch(c.Method.GetParameters().Count())
      {
        case 2:
          c.DynamicInvoke(new dynamic[] { Cast<I>(obj), Cast<T>(args) });
          break;
        case 1:
          c.DynamicInvoke(new dynamic[] { Cast<T>(args) });
          break;
        case 0:
          c.DynamicInvoke(new dynamic[] { });
          break;
        default:
          throw new Exception(
            $"Invalid number of parameters for {c.GetType().Name}.");
      }
    });

  //
  // EventHandler wrapper methods.
  //

  public string Name => name;

  public static EventProxy<I,T> operator +(EventProxy<I,T> e, Delegate c)
  {
    e.EventSubscribe(e.Name, e.ProxyTypedDelegate(c));
    return e;
  }

  public static EventProxy<I,T> operator -(EventProxy<I,T> e, Delegate c)
  {
    e.EventUnsubscribe(e.Name, e.ProxyTypedDelegate(c));
    return e;
  }
}