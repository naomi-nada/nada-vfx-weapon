using NADA.VFX.Weapons.Runtime;

namespace NADA.VFX.Runtime.Execution
{
    internal sealed class NadaModuleRunner
    {
        public void RunInitialApply(NadaWeaponRigContext context)
        {
            if (context == null || !context.IsValid)
                return;

            NadaWeaponRigOrchestrator.Run(context);
        }
    }
}