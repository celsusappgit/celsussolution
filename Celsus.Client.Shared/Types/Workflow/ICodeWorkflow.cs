using System.Collections.Generic;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types.Workflow
{
    public interface ICodeWorkflow
    {
        List<MyVariable> GlobalVariables { get; set; }
        List<MyArgument> GetArgumentsList();
        List<MyOption> GetOptionsList();
        
        void SetArguments(List<MyArgument> arguments);
        void SetOptions(List<MyOption> options);
        Task<bool> Run();
    }
}