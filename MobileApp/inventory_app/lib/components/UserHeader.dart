import 'package:flutter/material.dart';

class Userheader extends StatelessWidget {
  final String username;
  const Userheader({
    super.key,
    required this.username,
    });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16.0),
      child: Row(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(40.0),
            child: Image.asset(
              'assets/images/user.png',
              width: 40,
              height: 40,
              fit: BoxFit.cover,
            ),
          ),
          SizedBox(width: 10),
          Text('Hello, $username!', style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
          ),),
        ],
      ),
    );
  }
}
