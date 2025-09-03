import 'package:flutter/material.dart';
import 'package:inventory_app/components/my_button.dart';
import 'package:inventory_app/components/my_textfield.dart';
import 'package:inventory_app/pages/home_page.dart';
import 'package:inventory_app/services/apiservice.dart';

class LoginPage extends StatelessWidget {

  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _pwController = TextEditingController();
  final TextEditingController _ipController = TextEditingController();
  final Apiservice api = Apiservice();
  LoginPage({super.key});


  //login method
  void login(BuildContext context) {
    String email = _emailController.text;
    String password = _pwController.text;
    String ip = _ipController.text;
  // do zmiany miejsce ip
    api.auth(ip,email, password).then((response) {
      if (response.statusCode == 200) {
        // If the server returns an OK response, navigate to the home page
        Navigator.push(
          context,
          MaterialPageRoute(builder: (context) => const HomePage()),
        );
      } else {
        // If the server did not return a 200 OK response, show an error message
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Login failed: ${response.statusCode}')),
        );
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            //logo
            Icon(Icons.barcode_reader, 
            size: 60,
            color: Theme.of(context).colorScheme.primary,),
            const SizedBox(height:5),
            Text("Scanner App", style: TextStyle(
              fontSize: 35,
              color: Theme.of(context).colorScheme.primary
            ),),
            const SizedBox(height:150),
            //ip textfield
            MyTextfield(
              hintText: "IP Address i Port", 
              obscureText: false,
              controller: _ipController,
              ),

            //email textfield
            MyTextfield(
              hintText: "Email", 
              obscureText: false,
              controller: _emailController,
              ),

            const SizedBox(height: 10),
            //pw textfield
            MyTextfield(
              hintText: "Password", 
              obscureText: true,
              controller: _pwController,
              ),
            const SizedBox(height: 25),

            //login button
            MyButton(
              text: "Login",
              onTap: () {
                login(context);
              },
              ),

            const SizedBox(height: 25,),

            //ask for register
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text("Doesn't have an account? ", style: TextStyle(
                  color: Theme.of(context).colorScheme.primary
                  ),),
                Text("Register", style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: Theme.of(context).colorScheme.primary
                  ),
                ),
            ],)
          ],
        ),
      ),
    );
  }
}