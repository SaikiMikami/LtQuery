using LtQuery.Relational.Generators;

namespace LtQuery.Relational;

static class InjectParameterCache<TParameter>
{
    static InjectParameter<TParameter>? _value;
    public static InjectParameter<TParameter> GetValue(InjectParameterGenerator injectParameterGenerator)
    {
        var injectParameter = InjectParameterCache<TParameter>._value;
        if (injectParameter == null)
        {
            injectParameter = injectParameterGenerator.CreateInjectParameterFunc<TParameter>();
            InjectParameterCache<TParameter>._value = injectParameter;
        }
        return injectParameter;
    }
}
