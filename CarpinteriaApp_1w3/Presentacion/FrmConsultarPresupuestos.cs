using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarpinteriaApp_1w3.Datos;

namespace CarpinteriaApp_1w3.Presentacion
{
    public partial class FrmConsultarPresupuestos : Form
    {
        public FrmConsultarPresupuestos()
        {
            InitializeComponent();
        }

        private void FrmConsultarPresupuestos_Load(object sender, EventArgs e)
        {
            dtpDesde.Value = DateTime.Now.AddDays(-7);
            dtpHasta.Value = DateTime.Now;
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            // Falta validar datos
           List<Parametro> lista = new List<Parametro>();
            lista.Add(new Parametro("@fecha_desde",dtpDesde.Value.ToString("yyyy-MM-dd")));
            lista.Add(new Parametro("@fecha_hasta", dtpHasta.Value.ToString("yyyy-MM-dd")));
            lista.Add(new Parametro("@cliente",txtCliente.Text));
            DataTable tabla = new DBHelper().Consultar("SP_CONSULTAR_PRESUPUESTOS", lista);
            dgvPresupuestos.Rows.Clear();
            foreach (DataRow fila in tabla.Rows)
            {
                dgvPresupuestos.Rows.Add(new object[]
                {
                    fila["presupuesto_nro"].ToString(),
                    ((DateTime)fila["fecha"]).ToShortDateString(),
                    fila["cliente"].ToString(),
                    fila["total"].ToString()
                });
            
            }
        }
    }
}
