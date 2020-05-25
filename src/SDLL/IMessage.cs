using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SDLL
{
    /// <summary>
    /// Fournit un moyen de récupérer des informations sur un message d'informations ou d'une erreur.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Obtient le type du <see cref="System.Windows.MessageBoxImage"/> utilisé pour la <see cref="MessageBox"/>.
        /// </summary>
        MessageBoxImage MessageBoxImage { get; }

        /// <summary>
        /// Obtient le code d'erreur généré par l'application.
        /// </summary>
        string CodeErreur { get; }
        /// <summary>
        /// Obtient le message à afficher lorsqu'un évènement générant un message se produit. 
        /// </summary>
        string Information { get; }
    }
}
