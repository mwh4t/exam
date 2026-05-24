using korzunov.models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace korzunov
{
    public static class DbHelper
    {
        private static string _conn =
            "Server=localhost;Port=3306;Database=korzunov;Uid=root;Pwd=260612366002_idKM;" +
            "CharSet=utf8;AllowPublicKeyRetrieval=True;";

        public static User Login(string login, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT u.full_name, r.name FROM user u " +
                    "JOIN role r ON u.id_role = r.id_role " +
                    "WHERE u.login = @l AND u.password = @p", conn);
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@p", password);
                MySqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    User u = new User();
                    u.FullName = r.GetString(0);
                    u.RoleName = r.GetString(1);
                    return u;
                }
                return null;
            }
        }

        public static List<Product> GetProducts(string search, string supplier, int sortMode)
        {
            List<Product> list = new List<Product>();
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                string sql =
                    "SELECT p.article, p.name, p.description, p.price, p.discount, p.stock, " +
                    "c.name, s.name, m.name FROM product p " +
                    "JOIN category c ON p.id_category = c.id_category " +
                    "JOIN supplier s ON p.id_supplier = s.id_supplier " +
                    "JOIN manufacturer m ON p.id_manufacturer = m.id_manufacturer " +
                    // поиск по всем полям. Чтобы искать только по наименованию,
                    // заменить скобку с OR-ами на: "WHERE p.name LIKE @q "
                    "WHERE (p.article LIKE @q OR p.name LIKE @q OR p.description LIKE @q OR " +
                    "c.name LIKE @q OR s.name LIKE @q OR m.name LIKE @q) " +
                    "AND (@sup = 'Все поставщики' OR s.name = @sup)";

                if (sortMode == 1) sql += " ORDER BY p.stock ASC";
                else if (sortMode == 2) sql += " ORDER BY p.stock DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@q", "%" + search + "%");
                cmd.Parameters.AddWithValue("@sup", supplier);

                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    Product p = new Product();
                    p.Article = r.GetString(0);
                    p.Name = r.GetString(1);
                    p.Description = r.IsDBNull(2) ? "" : r.GetString(2);
                    p.Price = r.GetDecimal(3);
                    p.Discount = r.GetInt32(4);
                    p.Stock = r.GetInt32(5);
                    p.CategoryName = r.GetString(6);
                    p.SupplierName = r.GetString(7);
                    p.ManufacturerName = r.GetString(8);
                    list.Add(p);
                }
            }
            return list;
        }

        public static List<string> GetNames(string table)
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT name FROM " + table, conn);
                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(r.GetString(0));
            }
            return list;
        }

        public static bool IsInOrder(string article)
        {
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM order_item WHERE article = @a", conn);
                cmd.Parameters.AddWithValue("@a", article);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static int GetIdByName(MySqlConnection conn, string table, string idCol, string name)
        {
            MySqlCommand cmd = new MySqlCommand(
                "SELECT " + idCol + " FROM " + table + " WHERE name = @n", conn);
            cmd.Parameters.AddWithValue("@n", name);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void DeleteProduct(string article)
        {
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM product WHERE article = @a", conn);
                cmd.Parameters.AddWithValue("@a", article);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SaveProduct(Product p, bool isNew)
        {
            using (MySqlConnection conn = new MySqlConnection(_conn))
            {
                conn.Open();
                int cat = GetIdByName(conn, "category", "id_category", p.CategoryName);
                int sup = GetIdByName(conn, "supplier", "id_supplier", p.SupplierName);
                int man = GetIdByName(conn, "manufacturer", "id_manufacturer", p.ManufacturerName);

                MySqlCommand cmd;
                if (isNew)
                {
                    cmd = new MySqlCommand(
                        "INSERT INTO product (article, name, description, price, discount, stock, " +
                        "id_category, id_supplier, id_manufacturer, id_unit) " +
                        "VALUES (@a, @n, @d, @pr, @dc, @st, @c, @s, @m, 1)", conn);
                }
                else
                {
                    cmd = new MySqlCommand(
                        "UPDATE product SET name=@n, description=@d, price=@pr, discount=@dc, " +
                        "stock=@st, id_category=@c, id_supplier=@s, id_manufacturer=@m " +
                        "WHERE article=@a", conn);
                }
                cmd.Parameters.AddWithValue("@a", p.Article);
                cmd.Parameters.AddWithValue("@n", p.Name);
                cmd.Parameters.AddWithValue("@d", p.Description == null ? "" : p.Description);
                cmd.Parameters.AddWithValue("@pr", p.Price);
                cmd.Parameters.AddWithValue("@dc", p.Discount);
                cmd.Parameters.AddWithValue("@st", p.Stock);
                cmd.Parameters.AddWithValue("@c", cat);
                cmd.Parameters.AddWithValue("@s", sup);
                cmd.Parameters.AddWithValue("@m", man);
                cmd.ExecuteNonQuery();
            }
        }
    }
}