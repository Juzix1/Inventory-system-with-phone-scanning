import 'package:http/http.dart' as http;

class Apiservice {
  final client = http.Client();

  Future<http.Response> auth(String ip,String email, String pwd) async {
    final response = await client.post(Uri.parse("http://$ip/api/account/auth/"),
        headers: {
          "Content-Type": "application/json",
        },
        body: '{"email": "$email", "password": "$pwd"}');
    return response;
  }

  Future<http.Response> getById(String ip, String id) async {
    final response = await client.get(Uri.parse("http://$ip/api/Inventory/$id/"),
        headers: {
          "Content-Type": "application/json",
        });
    return response;
  }

}