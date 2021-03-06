﻿namespace VoxCake.IoC
{
    public interface IBinding
    {
        IEndBinding As<T>();
        IEndBinding As(object instance);
        
        IRawBinding Raw<T>();
        IRawBinding Raw(object instance);
        
        IEndBinding To<T>();
        IEndBinding To(object instance);
        
        void ToGlobalContainer();
    }
}