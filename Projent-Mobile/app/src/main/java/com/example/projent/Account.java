package com.example.projent;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.Toast;

import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.navigation.NavigationBarView;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class Account extends AppCompatActivity {

    BottomNavigationView bottomNavigationView;
    ApiInterface apiInterface;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_account);

        bottomNavigationView = findViewById(R.id.bottom_navigator);
        bottomNavigationView.setSelectedItemId(R.id.account);

        bottomNavigationView.setOnItemSelectedListener(new NavigationBarView.OnItemSelectedListener() {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item) {
                switch (item.getItemId()){
                    case R.id.dashboard:
                        startActivity(new Intent(getApplicationContext(),MainActivity.class));
                        overridePendingTransition(0,0);
                        return true;

                    case R.id.projects:
                        startActivity(new Intent(getApplicationContext(),Projects.class));
                        overridePendingTransition(0,0);
                        return true;


                    case R.id.account:
                        return true;
                }
                return false;
            }
        });


        apiInterface = RetrofitInstance.getRetrofit().create(ApiInterface.class);

        apiInterface.getUsers().enqueue(new Callback<List<RegistrationPojo>>() {
            @Override
            public void onResponse(Call<List<RegistrationPojo>> call, Response<List<RegistrationPojo>> response) {

                if (response.body().size()>0){
                }
                else {
                    Toast.makeText(Account.this,"User List is empty!", Toast.LENGTH_LONG).show();
                }

            }

            @Override
            public void onFailure(Call<List<RegistrationPojo>> call, Throwable t) {
                Toast.makeText(Account.this, t.getLocalizedMessage(), Toast.LENGTH_LONG).show();

            }
        });

        apiInterface.getProjects().enqueue(new Callback<List<ProjectsPojo>>() {
            @Override
            public void onResponse(Call<List<ProjectsPojo>> call, Response<List<ProjectsPojo>> response) {
                if (response.body().size()>0){
                }
                else {
                    Toast.makeText(Account.this,"Projects not Found!", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<List<ProjectsPojo>> call, Throwable t) {
                Toast.makeText(Account.this, t.getLocalizedMessage(), Toast.LENGTH_LONG).show();

            }
        });

        apiInterface.getMessages().enqueue(new Callback<List<MessagesPojo>>() {
            @Override
            public void onResponse(Call<List<MessagesPojo>> call, Response<List<MessagesPojo>> response) {
                if (response.body().size()>0){
                }
                else {
                    Toast.makeText(Account.this,"Messages not Found!", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<List<MessagesPojo>> call, Throwable t) {
                Toast.makeText(Account.this, t.getLocalizedMessage(), Toast.LENGTH_LONG).show();

            }
        });

        apiInterface.getAssignees().enqueue(new Callback<List<AssigneesPojo>>() {
            @Override
            public void onResponse(Call<List<AssigneesPojo>> call, Response<List<AssigneesPojo>> response) {
                if (response.body().size()>0){
                }
                else {
                    Toast.makeText(Account.this,"Projects not Found!", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<List<AssigneesPojo>> call, Throwable t) {
                Toast.makeText(Account.this, t.getLocalizedMessage(), Toast.LENGTH_LONG).show();

            }
        });
    }
}