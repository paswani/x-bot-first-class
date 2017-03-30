using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class
{
    public class LuisDialogFactory
    {
        private static List<LuisDialogBase<object>> _dialogs = null;

        private static List<LuisDialogBase<object>> Dialogs
        {
            get
            {
                if (_dialogs == null)
                {
                    EnsureDialogs();
                }
                return _dialogs;
            }
        }

        public async Task<LuisDialogBase<object>> Create(string query)
        {
            query = query.ToLowerInvariant();
            EnsureDialogs();

            foreach (var resourceDialog in Dialogs)
            {
                if (await resourceDialog.CanHandle(query))
                {
                    return resourceDialog;
                }
            }
            return null;
        }

        private static void EnsureDialogs()
        {
            _dialogs = new List<LuisDialogBase<object>>();
            var types = typeof(LuisDialogFactory).Assembly.GetTypes().Where(type => type.IsClass && type.BaseType == typeof(LuisDialogBase<object>)).ToList();
            types.ForEach(type =>
            {
                _dialogs.Add((LuisDialogBase<object>)Activator.CreateInstance(type));
            });
        }
    }
}
