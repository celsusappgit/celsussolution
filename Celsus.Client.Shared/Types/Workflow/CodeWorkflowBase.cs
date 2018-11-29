using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types.Workflow
{
    public class CodeWorkflowBase
    {
        protected List<MyArgument> arguments;
        protected List<MyOption> options;

        public List<MyVariable> GlobalVariables { get; set; }
        protected T GetArgument<T>(string name)
        {
            var arg = arguments.SingleOrDefault(x => x.Name == name);
            if (arg != null)
            {
                if (string.IsNullOrWhiteSpace(arg.JSonValue) == false)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<T>(arg.JSonValue);
                        return result;
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
                else
                {
                    return default(T);
                }
            }
            return default(T);
        }

        protected T GetVariable<T>(string name)
        {
            var variable = GlobalVariables.SingleOrDefault(x => x.Name == name);
            if (variable != null)
            {
                if (string.IsNullOrWhiteSpace(variable.JSonValue) == false)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<T>(variable.JSonValue);
                        return result;
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
                else
                {
                    return default(T);
                }
            }
            return default(T);
        }

        protected T GetOption<T>(string name)
        {
            if (options == null)
            {
                return default(T);
            }
            var opt = options.SingleOrDefault(x => x.Name == name);
            if (opt != null)
            {
                if (string.IsNullOrWhiteSpace(opt.JSonValue) == false)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<T>(opt.JSonValue);
                        return result;
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
                else
                {
                    return default(T);
                }
            }
            return default(T);
        }
    }
}
