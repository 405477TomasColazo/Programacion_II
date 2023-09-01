using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CarpinteriaApp_1w3.Entidades;

namespace CarpinteriaApp_1w3.Datos
{
    internal class DBHelper
    {
        private SqlConnection conexion;
        public DBHelper()
        {
            conexion = new SqlConnection(@"Data Source=172.16.10.196;Initial Catalog=Carpinteria_2023;User ID=alumno1w1; Password=alumno1w1");
        }

        public int ProximoPresupuesto()
        {
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = "SP_PROXIMO_ID";
            SqlParameter parametro = new SqlParameter();
            parametro.ParameterName = "@next";
            parametro.SqlDbType = SqlDbType.Int;
            parametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(parametro);
            comando.ExecuteNonQuery();
            conexion.Close();
            return Convert.ToInt32(parametro.Value);
        }
        public DataTable Consultar(string sp)
        {
            DataTable dt = new DataTable();
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = sp;
            dt.Load(comando.ExecuteReader());
            conexion.Close();
            return dt;
        }
        public DataTable Consultar(string sp,List<Parametro> lstParametros)
        {
            DataTable dt = new DataTable();
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = sp;
            foreach (Parametro p in lstParametros)
            {
                comando.Parameters.AddWithValue(p.Nombre, p.Valor);
            }
            dt.Load(comando.ExecuteReader());
            conexion.Close();
            return dt;
        }

        public bool ConfirmarPresupuesto(Presupuesto oPresupuesto)
        {
            bool resultado = true;
            SqlTransaction t = null;
            try
            {
                conexion.Open();
                t = conexion.BeginTransaction();
                SqlCommand comando = new SqlCommand();
                comando.Connection = conexion;
                comando.Transaction = t;
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = "SP_INSERTAR_MAESTRO";
                comando.Parameters.AddWithValue("@cliente", oPresupuesto.Cliente);
                comando.Parameters.AddWithValue("@dto", oPresupuesto.Descuento);
                comando.Parameters.AddWithValue("@total", oPresupuesto.CalcularTotal());
                SqlParameter parametro = new SqlParameter();
                parametro.ParameterName = "@presupuesto_nro";
                parametro.SqlDbType = SqlDbType.Int;
                parametro.Direction = ParameterDirection.Output;
                comando.Parameters.Add(parametro);
                comando.ExecuteNonQuery();
                int presupuestoNro = (int)parametro.Value;
                int detalleNro = 1;
                SqlCommand cmdDetalle;
                foreach (DetallePresupuesto d in oPresupuesto.Detalles)
                {
                    cmdDetalle = new SqlCommand("SP_INSERTAR_DETALLE", conexion, t);
                    cmdDetalle.CommandType = CommandType.StoredProcedure;
                    cmdDetalle.Parameters.AddWithValue("@presupuesto_nro", presupuestoNro);
                    cmdDetalle.Parameters.AddWithValue("@detalle", detalleNro);
                    cmdDetalle.Parameters.AddWithValue("@id_producto", d.Producto.ProductoNro);
                    cmdDetalle.Parameters.AddWithValue("@cantidad", d.Cantidad);
                    cmdDetalle.ExecuteNonQuery();
                    detalleNro++;
                }
                t.Commit();
            }
            catch
            {
                t.Rollback();
                resultado = false;
            }
            finally
            {
                if(conexion!= null && conexion.State == ConnectionState.Open)
                { conexion.Close(); }
                
            }
            return resultado;
        }
    }
}
