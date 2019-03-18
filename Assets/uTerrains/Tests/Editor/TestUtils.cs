using System;
using System.Reflection;

namespace UltimateTerrains
{
    public static class TestUtils
    {
        public class PrivateConstructor<T>
        {
            ConstructorInfo constructor;

            public PrivateConstructor(ConstructorInfo constructor)
            {
                this.constructor = constructor;
            }

            public T WithParams(params object[] paramsValues)
            {
                return (T) constructor.Invoke(paramsValues);
            }

            public T WithoutParam()
            {
                return WithParams();
            }
        }

        public class PrivateMethod<T>
        {
            MethodInfo method;
            object instance;

            public PrivateMethod(MethodInfo method, object instance)
            {
                this.method = method;
                this.instance = instance;
            }

            public T WithParams(params object[] paramsValues)
            {
                return (T) method.Invoke(instance, paramsValues);
            }

            public T WithoutParam()
            {
                return WithParams();
            }
        }

        public class PrivateMethodVoid
        {
            MethodInfo method;
            object instance;

            public PrivateMethodVoid(MethodInfo method, object instance)
            {
                this.method = method;
                this.instance = instance;
            }

            public void WithParams(params object[] paramsValues)
            {
                method.Invoke(instance, paramsValues);
            }

            public void WithoutParam()
            {
                WithParams();
            }
        }

        public static PrivateConstructor<T> CallPrivateConstructor<T>(params Type[] argsType)
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, argsType, null);
            return new PrivateConstructor<T>(constructor);
        }

        public static PrivateMethod<T> CallPrivateMethod<T>(object instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return new PrivateMethod<T>(method, instance);
        }

        public static PrivateMethodVoid CallPrivateMethodVoid(object instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return new PrivateMethodVoid(method, instance);
        }

        public static T GetPrivateFieldValue<T>(object instance, string fieldName)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            return (T) field.GetValue(instance);
        }

        public static void SetPrivateFieldValue(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            field.SetValue(instance, value);
        }
    }
}