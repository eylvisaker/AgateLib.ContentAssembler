pipeline {
    agent {
        label 'windows'
    }

    options {
        skipDefaultCheckout(true)
    }

    stages {
        stage('Checkout') {
            steps {
                deleteDir()
                checkout scm
            }
        }
        stage('Build') {
            steps {
                powershell './build.ps1 Compile -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
            }
        }
        stage('Test') {
            steps {
                powershell './build.ps1 Test -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Package') {
            steps {
                powershell './build.ps1 Pack -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Publish') {
            withCredentials([string(credentialsId: 'nuget-api-key', variable: 'NUGET_APIKEY')]) {
                powershell './build.ps1 Push -nugetapi $env:NUGET_APIKEY -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
    }

    post {
        always {
            nunit testResultsPattern: 'artifacts/**/tests/*.xml'
        }
        failure {
            script {
                mail (
          to: 'eylvisaker@gmail.com',
          subject: "FAILED BUILD - AgateLib.ContentAssembler ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
          body: "Generations of Lore branch ${env.BRANCH_NAME} failed to build."
        )
            }
        }
        fixed {
            script {
                mail (
          to: 'eylvisaker@gmail.com',
          subject: "FIXED BUILD - AgateLib.ContentAssembler ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
          body: "Generations of Lore branch ${env.BRANCH_NAME} is fixed now."
        )
            }
        }
        success {
            script {
                cleanWs()
            }
        }
    }
}
