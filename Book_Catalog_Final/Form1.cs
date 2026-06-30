using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Book_Catalog_Final
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void lstBooks_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBoxItem selectedItem = lstBooks.SelectedItem as ListBoxItem;

            if (selectedItem == null) return;

            string isbn = selectedItem.Isbn;

            if (string.IsNullOrEmpty(isbn))
            {
                picCover.Image = null;
                lblStatus.Text = "Для этой книги нет ISBN, обложка недоступна";
                return;
            }

            lblStatus.Text = "Загрузка обложки...";
            picCover.Image = null;

            string coverUrl = $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "MyBookApp (aleksei.ilin.07@mail.ru)");
                try
                {
                    byte[] imageData = httpClient.GetByteArrayAsync(coverUrl).Result;

                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        picCover.Image = Image.FromStream(ms);
                    }

                    lblStatus.Text = $"Обложка загружена для: {selectedItem.DisplayText}";
                }
                catch (HttpRequestException)
                {
                    lblStatus.Text = "Обложка не найдена";
                    picCover.Image = null;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"Ошибка загрузки обложки: {ex.Message}";
                    picCover.Image = null;
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введите название книги", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            lstBooks.Items.Clear();
            picCover.Image = null;
            lblStatus.Text = "Загрузка...";

            string encodedQuery = Uri.EscapeDataString(query);

            string url = $"https://openlibrary.org/search.json?q={encodedQuery}&fields=title,author_name,first_publish_year,isbn&limit=50";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "MyBookApp (aleksei.ilin.07@mail.ru)");

                try
                {
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string content = response.Content.ReadAsStringAsync().Result;
                        JObject jsonResponse = JObject.Parse(content);
                        JArray docs = (JArray)jsonResponse["docs"];

                        if (docs != null && docs.Count > 0)
                        {
                            foreach (JObject book in docs)
                            {
                                string title = book["title"]?.ToString() ?? "Без названия";

                                string authors = "Автор неизвестен";
                                if (book["author_name"] is JArray authorArray && authorArray.Count > 0)
                                {

                                    authors = string.Join(", ", authorArray);
                                }

                                string year = book["first_publish_year"]?.ToString() ?? "?";
                                string displayText = $"{title} - {authors} ({year})";

                                string isbn = null;
                                if (book["isbn"] is JArray isbnArray && isbnArray.Count > 0)
                                {
                                    isbn = isbnArray[0].ToString();
                                }

                                var item = new ListBoxItem
                                {
                                    DisplayText = displayText,
                                    Isbn = isbn
                                };
                                lstBooks.Items.Add(item);
                            }

                            lblStatus.Text = $"Найдено книг: {docs.Count}";
                        }
                        else
                        {
                            lblStatus.Text = "По вашему запросу ничего не найдено.";
                        }
                    }
                    else
                    {
                        lblStatus.Text = $"Ошибка сервера: {response.StatusCode}";
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    lblStatus.Text = $"Ошибка сети: {httpEx.Message}";
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"Неизвестная ошибка: {ex.Message}";
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstBooks.Items.Clear();
            picCover.Image = null;
            txtSearch.Text = "";
            lblStatus.Text = "Готово. Список очищен.";
        }
    }
}
// Комментарий о том, что я закончил написание моего REST API проекта.