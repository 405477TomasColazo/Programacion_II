using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Data.SqlClient;
using CarpinteriaApp_1w3.Entidades;
using CarpinteriaApp_1w3.Datos;

namespace CarpinteriaApp_1w3.Presentacion
{
    public partial class FrmNuevoPresupuesto : Form
    {
        Presupuesto nuevo;
        DBHelper gestor;
        public FrmNuevoPresupuesto()
        {
            InitializeComponent();
            nuevo = new Presupuesto();
            gestor = new DBHelper();
        }

        private void FrmNuevoPresupuesto_Load(object sender, EventArgs e)
        {
            txtFecha.Text = DateTime.Today.ToShortDateString();
            txtCliente.Text = "Consumidor Final";
            txtDescuento.Text = "0";
            txtCantidad.Text = "1";
            lblPresupuestoNro.Text += " " + gestor.ProximoPresupuesto();
            //ProximoPresupuesto();
            CargarProductos();
        }

        private void CargarProductos()
        {
            /*SqlConnection conexion = new SqlConnection();
            conexion.ConnectionString = @"Data Source=172.16.10.196;Initial Catalog=Carpinteria_2023;User ID=alumno1w1; Password=alumno1w1";
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = "SP_CONSULTAR_PRODUCTOS";
            DataTable tabla = new DataTable();
            tabla.Load(comando.ExecuteReader());
            conexion.Close();*/
            cboProducto.DataSource = gestor.Consultar("SP_CONSULTAR_PRODUCTOS");
            cboProducto.ValueMember = "id_producto";
            cboProducto.DisplayMember = "n_producto";
        }

        /*private void ProximoPresupuesto()
        {
            SqlConnection conexion = new SqlConnection();
            conexion.ConnectionString = @"Data Source=172.16.10.196;Initial Catalog=Carpinteria_2023;User ID=alumno1w1; Password=alumno1w1";
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection= conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = "SP_PROXIMO_ID";
            SqlParameter parametro = new SqlParameter();
            parametro.ParameterName = "@next";
            parametro.SqlDbType= SqlDbType.Int;
            parametro.Direction= ParameterDirection.Output;
            comando.Parameters.Add(parametro);
            comando.ExecuteNonQuery();
            conexion.Close();
            lblPresupuestoNro.Text += " " + parametro.Value.ToString();
        }*/

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (cboProducto.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un producto...",
                                "Control",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtCantidad.Text) || !int.TryParse(txtCantidad.Text,out _) )
            {
                MessageBox.Show("Debe ingresar una cantidad valida...",
                                                "Control",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Exclamation);
                return;
            }
            foreach (DataGridViewRow fila in dgvDetalles.Rows)
            {
                if (fila.Cells["colProducto"].Value.ToString().Equals(cboProducto.Text))
                {
                    MessageBox.Show("Este producto ya esta presupuestado...",
                                "Control",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                    return;
                }
            }
            DataRowView item = (DataRowView)cboProducto.SelectedItem;
            int nro = Convert.ToInt32(item.Row.ItemArray[0]);
            string nom = item.Row.ItemArray[1].ToString();
            double pre = Convert.ToDouble(item.Row.ItemArray[2]);
            Producto p = new Producto(nro,nom,pre);
            int cant = Convert.ToInt32(txtCantidad.Text);
            DetallePresupuesto detalle = new DetallePresupuesto(p,cant);
            nuevo.AgregarDetalle(detalle);
            dgvDetalles.Rows.Add(detalle.Producto.ProductoNro,
                                detalle.Producto.Nombre,
                                detalle.Producto.Precio,
                                detalle.Cantidad,
                                "Quitar");
            CalcularTotales();
        }

        private void CalcularTotales()
        {
            double total = nuevo.CalcularTotal();
            txtSubTotal.Text = total.ToString();
            double dto = total * Convert.ToDouble(txtDescuento.Text)/100;
            txtTotal.Text = (total-dto).ToString();
        }

        private void dgvDetalles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           /* if (dgvDetalles.CurrentCell.ColumnIndex == 4)
            {
                nuevo.QuitarDetalle(dgvDetalles.CurrentRow.Index);
                dgvDetalles.Rows.RemoveAt(dgvDetalles.CurrentRow.Index);
                CalcularTotales();
            }*/
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                nuevo.QuitarDetalle(e.RowIndex);
                dgvDetalles.Rows.RemoveAt(e.RowIndex);
                CalcularTotales();
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtCliente.Text))
            {
                MessageBox.Show("Debe ingresar un cliente...",
                                "Control",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return;
            }
            if(dgvDetalles.Rows.Count == 0)
            {
                MessageBox.Show("Debe ingresar al menos un producto...",
                                                "Control",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Exclamation);
                return;
            }
            GrabarPresupuesto();
        }

        private void GrabarPresupuesto()
        {
            nuevo.Fecha = Convert.ToDateTime(txtFecha.Text);
            nuevo.Cliente = txtCliente.Text;
            nuevo.Descuento = Convert.ToDouble(txtDescuento.Text);
            if (gestor.ConfirmarPresupuesto(nuevo))
            {
                MessageBox.Show("Se grabo con exito el presupuesto...",
                                                "Informe",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                this.Dispose();
            }
            else
            {
                MessageBox.Show("No se pudo grabar el presupuesto...",
                                                "Error",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
