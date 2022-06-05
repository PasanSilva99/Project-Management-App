package com.example.projent;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

public class Registration extends AppCompatActivity {

    Button btnBackR;
    Button btnAddPhotoR;
    Button btnRegisterR;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_registration);

        btnBackR = findViewById(R.id.btnBackR);
        btnAddPhotoR = findViewById(R.id.btnAddPhotoR);
        btnRegisterR = findViewById(R.id.btnRegisterR);

        btnBackR.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                startActivity(new Intent(getApplicationContext(), LogIn.class));
            }
        });

        btnRegisterR.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                startActivity(new Intent(getApplicationContext(), LogIn.class));
            }
        });

    }
}