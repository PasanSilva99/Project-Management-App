package com.example.projent_mobile;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import com.example.projent_mobile.ui.dashboard.DashboardFragment;

public class Register extends AppCompatActivity {
    private Button btnBackR;
    private Button btnRegisterR;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        btnBackR = findViewById(R.id.btnBackR);
        btnRegisterR = findViewById(R.id.btnRegisterR);

        btnBackR.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(Register.this, LogIn.class);
                startActivity(intent);
            }
        });

        btnRegisterR.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(Register.this, DashboardFragment.class);
                startActivity(intent);
            }
        });
    }
}