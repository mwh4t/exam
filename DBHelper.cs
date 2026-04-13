using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace exam
{
    public static class DbHelper
    {
        // ← поменяй Pwd на экзамене
        private static string _conn =
            "Server=localhost;Port=3306;Database=shoes;Uid=root;Pwd=260612366002_idKM;" +
            "CharSet=utf8;AllowPublicKeyRetrieval=True;";

        private static MySqlConnection GetConn()
        {
            return new MySqlConnection(_conn);
        }

        public static User Login(string login, string password)
        {
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT u.full_name, r.name " +
                    "FROM user u JOIN role r ON u.id_role = r.id_role " +
                    "WHERE u.login = @l AND u.password = @p", conn);
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@p", password);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    User u = new User();
                    u.FullName = reader.GetString(0);
                    u.RoleName = reader.GetString(1);
                    return u;
                }
                return null;
            }
        }

        public static List<Product> GetProducts()
        {
            List<Product> list = new List<Product>();
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT p.article, p.name, p.description, p.price, p.discount, " +
                    "p.stock, p.photo, c.name, s.name, m.name, u.name " +
                    "FROM product p " +
                    "JOIN category c ON p.id_category = c.id_category " +
                    "JOIN supplier s ON p.id_supplier = s.id_supplier " +
                    "JOIN manufacturer m ON p.id_manufacturer = m.id_manufacturer " +
                    "JOIN unit u ON p.id_unit = u.id_unit", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Product p = new Product();
                    p.Article = reader.GetString(0);
                    p.Name = reader.GetString(1);
                    p.Description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    p.Price = reader.GetDecimal(3);
                    p.Discount = reader.GetInt32(4);
                    p.Stock = reader.GetInt32(5);
                    p.Photo = reader.IsDBNull(6) ? null : reader.GetString(6);
                    p.CategoryName = reader.GetString(7);
                    p.SupplierName = reader.GetString(8);
                    p.ManufacturerName = reader.GetString(9);
                    p.UnitName = reader.GetString(10);
                    list.Add(p);
                }
            }
            return list;
        }

        public static List<string> GetSuppliers()
        {
            List<string> list = new List<string>();
            list.Add("Все поставщики");
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT name FROM supplier", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader.GetString(0));
            }
            return list;
        }

        public static List<string> GetCategories()
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT name FROM category", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader.GetString(0));
            }
            return list;
        }

        public static List<string> GetManufacturers()
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT name FROM manufacturer", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader.GetString(0));
            }
            return list;
        }

        public static bool IsProductInOrder(string article)
        {
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM order_item WHERE article = @a", conn);
                cmd.Parameters.AddWithValue("@a", article);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void DeleteProduct(string article)
        {
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "DELETE FROM product WHERE article = @a", conn);
                cmd.Parameters.AddWithValue("@a", article);
                cmd.ExecuteNonQuery();
            }
        }

        public static void AddProduct(Product p)
        {
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                int idCat = GetIdByName(conn, "category", "id_category", p.CategoryName);
                int idSup = GetIdByName(conn, "supplier", "id_supplier", p.SupplierName);
                int idMan = GetIdByName(conn, "manufacturer", "id_manufacturer", p.ManufacturerName);

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO product (article, name, description, price, discount, stock, " +
                    "photo, id_category, id_supplier, id_manufacturer, id_unit) " +
                    "VALUES (@art, @name, @desc, @price, @disc, @stock, @photo, @cat, @sup, @man, 1)",
                    conn);
                cmd.Parameters.AddWithValue("@art", p.Article);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@desc", p.Description);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@disc", p.Discount);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@photo", p.Photo == null ? (object)DBNull.Value : p.Photo);
                cmd.Parameters.AddWithValue("@cat", idCat);
                cmd.Parameters.AddWithValue("@sup", idSup);
                cmd.Parameters.AddWithValue("@man", idMan);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateProduct(Product p)
        {
            using (MySqlConnection conn = GetConn())
            {
                conn.Open();
                int idCat = GetIdByName(conn, "category", "id_category", p.CategoryName);
                int idSup = GetIdByName(conn, "supplier", "id_supplier", p.SupplierName);
                int idMan = GetIdByName(conn, "manufacturer", "id_manufacturer", p.ManufacturerName);

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE product SET name=@name, description=@desc, price=@price, " +
                    "discount=@disc, stock=@stock, photo=@photo, " +
                    "id_category=@cat, id_supplier=@sup, id_manufacturer=@man " +
                    "WHERE article=@art", conn);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@desc", p.Description);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@disc", p.Discount);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@photo", p.Photo == null ? (object)DBNull.Value : p.Photo);
                cmd.Parameters.AddWithValue("@cat", idCat);
                cmd.Parameters.AddWithValue("@sup", idSup);
                cmd.Parameters.AddWithValue("@man", idMan);
                cmd.Parameters.AddWithValue("@art", p.Article);
                cmd.ExecuteNonQuery();
            }
        }

        private static int GetIdByName(MySqlConnection conn, string table, string idCol, string name)
        {
            MySqlCommand cmd = new MySqlCommand(
                "SELECT " + idCol + " FROM " + table + " WHERE name = @n", conn);
            cmd.Parameters.AddWithValue("@n", name);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}

